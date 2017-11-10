using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ragnar;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using Ragnar;

namespace TVSPlayer {
    class TorrentDownloader {

        public static List<TorrentDownloader> torrents = new List<TorrentDownloader>();

        public TorrentDownloader(Torrent torrent) {
            TorrentSource = torrent;
        }
        public EventHandler DownloadFinished;
        public TorrentStatus Status { get; set; }
        public Torrent TorrentSource { get; set; }
        public TorrentHandle Handle { get; set; }
        public Session TorrentSession { get; set; }

        private async Task Download(bool sequential) {
             await Task.Run(() => {
                 TorrentSession = new Session();
                 TorrentSession.ListenOn(6881, 6889);
                 var torrentParams = new AddTorrentParams();
                 torrentParams.SavePath = GetDownloadPath();
                 torrentParams.Url = TorrentSource.Magnet;
                 Handle = TorrentSession.AddTorrent(torrentParams);
                 Handle.SequentialDownload = sequential;
                 Status = Handle.QueryStatus();
                 
                 torrents.Add(this);
            });
            
        }


        public async Task<TorrentStatus> Stream() {
            await Download(true);
            return Status;
        }

        public async Task Download() {
            await Download(false);
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
