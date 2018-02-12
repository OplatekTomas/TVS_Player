using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ragnar;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using System.Threading;
using TVS.Notification;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using TVS.API;
using static TVS.API.Episode;
using System.Windows;
using HtmlAgilityPack;

namespace TVSPlayer {
    public class TorrentDownloader {

        /// <summary>
        /// Default constructor for TorrentDownloader
        /// </summary>
        /// <param name="torrent">Torrent that is supposed to be downloaded</param>
        public TorrentDownloader(Torrent torrent) {
            TorrentSource = torrent;
        }

        private static List<TorrentDownloader> torrents = new List<TorrentDownloader>();


        public static Session TorrentSession;
        public TorrentStatus Status { get; set; }
        public Torrent TorrentSource { get; set; }
        public TorrentHandle Handle { get; set; }
        public bool ShowNotificationWhenFinished { get; set; } = true;
        public bool IsPaused { get; set; } = false;

        /// <summary>
        /// Returns all torrents that are being downloaded right now
        /// </summary>
        /// <returns></returns>
        public static List<TorrentDownloader> GetTorrents() {
            return torrents;
        }

        /// <summary>
        /// Starts streaming process for torrent and shows player
        /// </summary>
        /// <param name="showStream">Show player when stream starts</param>
        /// <returns></returns>
        public async Task<TorrentDownloader> Stream(bool showStream = true) {
            var torrs = torrents.Where(x => x.TorrentSource.Magnet == TorrentSource.Magnet).ToList();
            if (torrs.Count == 0) {
                if (!Settings.StreamedBefore) {
                    await MessageBox.Show("Streaming is not fully stable feature.\n\nIt still works via torrent which means that files are not always perfect while downloading.\nIf you experience glitches try moving video timeline slider.\nIf that doesn't help just restart the stream", "Streaming tips");
                    Settings.StreamedBefore = true;
                }
                var downloader = await Task.Run(() => {
                    return DownloadLocal(true);
                });
#pragma warning disable CS4014
                Task.Run(() => {
                    while (Handle != null && !downloader.Status.IsSeeding) {
                        Trace.WriteLine(downloader.Status.DownloadRate + ", " + downloader.Status.AllTimeDownload);
                        downloader.Status = downloader.Handle.QueryStatus();
                        Thread.Sleep(1000);
                    }
                });
#pragma warning restore CS4014
                GC.Collect();
                MainWindow.AddPage(new TorrentStreamer(downloader));
                return downloader;
            } else {
                await MessageBox.Show("Torrnet is already downloading");
                return null;
            }
        }

        /// <summary>
        /// Download torrent, its added to database when download has finished
        /// </summary>
        /// <returns></returns>
        public async Task<TorrentDownloader> Download() {
            if (String.IsNullOrEmpty(TorrentSource.Magnet)) { 
                await Task.Run(() => {
                    HtmlWeb htmlWeb = new HtmlWeb();
                    HtmlDocument htmlDocument = htmlWeb.Load(TorrentSource.URL);
                    List<HtmlNode> a = htmlDocument.DocumentNode.SelectNodes("//ul").ToList();
                    TorrentSource.Magnet = a[5].ChildNodes[7].ChildNodes[0].Attributes[1].Value;
                });
            }
            var torrs = torrents.Where(x => x.TorrentSource.Magnet == TorrentSource.Magnet).ToList();
            if (torrs.Count == 0) {
                var downloader = await Task.Run(() => {
                    return DownloadLocal(false);
                });
#pragma warning disable CS4014
                Task.Run(() => {
                    while (Handle != null && !downloader.Status.IsSeeding) {
                        Trace.WriteLine(downloader.Status.DownloadRate + ", " + downloader.Status.AllTimeDownload);
                        downloader.Status = downloader.Handle.QueryStatus();
                        Thread.Sleep(1000);
                    }
                    if (downloader.Status != null) {
                        StopAndMove();
                        if (ShowNotificationWhenFinished) {
                            NotificationSender sender = new NotificationSender("Download finished", Helper.GenerateName(TorrentSource.Series, TorrentSource.Episode));
                            sender.ClickedEvent += (s, ev) => {
                                Application.Current.Dispatcher.Invoke(() => {
                                    PlayFile(TorrentSource.Series, Database.GetEpisode(TorrentSource.Series.id, TorrentSource.Episode.id));
                                }, DispatcherPriority.Send);
                            };
                            sender.Show();
                        }
                    }
                });
#pragma warning restore CS4014

                return downloader;
            } else {
                await MessageBox.Show("Torrent is already downloading");
                return null;
            }
        }

        /// <summary>
        /// Sets download speed limit in kB/s. 0 = unlimited
        /// </summary>
        /// <param name="limit"></param>
        public static void SetDownloadSpeedLimit(int limit) {
            Settings.DownloadSpeed = limit;
            if (TorrentSession != null) {
                SessionSettings ss = TorrentSession.QuerySettings();
                ss.DownloadRateLimit = limit*1024;
                TorrentSession.SetSettings(ss);
            }

        }

        /// <summary>
        /// Sets upload speed limit in kB/s. 0 = unlimited
        /// </summary>
        /// <param name="limit"></param>
        public static void SetUploadSpeedLimit(int limit) {
            Settings.UploadSpeed = limit;
            if (TorrentSession != null) {
                SessionSettings ss = TorrentSession.QuerySettings();
                ss.UploadRateLimit = limit*1024;
                TorrentSession.SetSettings(ss);
            }

        }

        /// <summary>
        /// Pauses torrent download
        /// </summary>
        public async void Pause() {
            IsPaused = true;
            await Task.Run(() => {
                while (IsPaused) {
                    Handle.Pause();
                    Thread.Sleep(100);
                }
            });
        }

        /// <summary>
        /// Resumes torrent download
        /// </summary>
        public void Resume() {
            IsPaused = false;
        }

        /// <summary>
        /// Stop torrent from being active
        /// </summary>
        /// <param name="deleteFiles"></param>
        public void Remove(bool deleteFiles) {
            string magnet = TorrentSource.Magnet;
            TorrentSession.RemoveTorrent(Handle, deleteFiles);
            TorrentDatabase.Remove(magnet);
            torrents.Remove(this);
        }

        /// <summary>
        /// Stops torrent and moves it to the right directory
        /// </summary>
        public async void StopAndMove() {
            TorrentSource.Name = Handle.TorrentFile.Name;
            await Renamer.RenameAfterDownload(this);
            TorrentSource.FinishedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            TorrentSource.HasFinished = true;
            TorrentDatabase.Edit(TorrentSource.Magnet, TorrentSource);
            torrents.Remove(this);
        }

        /// <summary>
        /// Continues all unifinished torrents
        /// </summary>
        public async static void ContinueUnfinished() {
            var torrents = TorrentDatabase.Load();
            torrents = torrents.Where(x => x.HasFinished == false).ToList();
            foreach (var item in torrents) {
                TorrentDatabase.Remove(item.Magnet);
                TorrentDownloader downloader = new TorrentDownloader(item);
                await downloader.Download();
            }
        }

        private TorrentDownloader DownloadLocal(bool sequential) {
            if (TorrentSession == null) {
                TorrentSession = new Session();
                TorrentSession.ListenOn(6881, 6889);
            }      
            var torrentParams = new AddTorrentParams();
            torrentParams.SavePath = GetDownloadPath();
            torrentParams.Url = TorrentSource.Magnet;
            Handle = TorrentSession.AddTorrent(torrentParams);
            SetDownloadSpeedLimit(Settings.DownloadSpeed);
            SetUploadSpeedLimit(Settings.UploadSpeed);
            Handle.SequentialDownload = TorrentSource.IsSequential = sequential;
            Status = Handle.QueryStatus();
            torrents.Add(this);
            TorrentDatabase.Save(TorrentSource);
            return this;
        }

        private async void PlayFile(Series series, Episode episode) {
            List<Episode.ScannedFile> list = new List<Episode.ScannedFile>();
            episode = Database.GetEpisode(series.id, episode.id);
            foreach (var item in episode.files) {
                if (item.Type == Episode.ScannedFile.FileType.Video) {
                    list.Add(item);
                }
            }
            List<FileInfo> infoList = new List<FileInfo>();
            foreach (var item in list) {
                infoList.Add(new FileInfo(item.NewName));
            }
            FileInfo info = infoList.OrderByDescending(ex => ex.Length).FirstOrDefault();
            if (info != null) {
                ScannedFile sf = list.Where(x => x.NewName == info.FullName).FirstOrDefault();
                if (!Settings.UseWinDefaultPlayer) {
                    //Used to release as many resources as possible to give all rendering power to video playback
                    MainWindow.RemoveAllPages();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    await Task.Run(() => {
                        Thread.Sleep(500);
                    });
                    MainWindow.AddPage(new LocalPlayer(series, episode, sf));
                } else {
                    Process.Start(sf.NewName);
                }
            }
        }

        private string GetDownloadPath() {
            if (!String.IsNullOrEmpty(Settings.DownloadCacheLocation)) {
                return Settings.DownloadCacheLocation;
            } else {
                string downloads = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();      
                var dialog = new VistaFolderBrowserDialog();
                dialog.SelectedPath = downloads + "\\Select Path";
                if ((bool)dialog.ShowDialog()) {
                    Settings.DownloadCacheLocation = dialog.SelectedPath;
                    return dialog.SelectedPath;
                } else {
                    return downloads;
                }
            }
        }

    }
}
