using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;
using TVS.Notification;
using static TVS.API.Episode;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for EpisodeDetails.xaml
    /// </summary>
    public partial class EpisodeDetails : UserControl
    {
        public EpisodeDetails(Episode episode)
        {
            InitializeComponent();
            this.episode = episode;
        }

        Episode episode;

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            EpisodeThumb.Source = await Database.GetEpisodeThumbnail(Int32.Parse(episode.seriesId.ToString()), episode.id);
            EPName.Text = EpisodeName.Text = episode.episodeName;
            Season.Text = Helper.GenerateName(episode);
            Rating.Text = episode.siteRating + "/10";
            if (!String.IsNullOrEmpty(episode.firstAired)) {
                Airdate.Text = DateTime.ParseExact(episode.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd. MM. yyyy");
            }
            Writers.Text = null;
            for (int i = 0; i < episode.writers.Count; i++) {
                if (i == episode.writers.Count - 1) {
                    Writers.Text += episode.writers[i];
                } else {
                    Writers.Text += episode.writers[i] + ", ";
                }
            }
            Directors.Text = null;
            for (int i = 0; i < episode.directors.Count; i++) {
                if (i == episode.directors.Count - 1) {
                    Directors.Text += episode.directors[i];
                } else {
                    Directors.Text += episode.directors[i] + ", ";
                }
            }
            GuestStars.Text = null;
            for (int i = 0; i < episode.guestStars.Count; i++) {
                if (i == episode.guestStars.Count - 1) {
                    GuestStars.Text += episode.guestStars[i];
                } else {
                    GuestStars.Text += episode.guestStars[i] + ", ";
                }
            }
            if (episode.files.Count > 0) {
                foreach (var item in episode.files) {
                    if (item.Type == TVS.API.Episode.ScannedFile.FileType.Video) {
                        Downloaded.Text = "Yes";
                        break;
                    }
                }
            }
            Overview.Text = episode.overview;
        }

        private void EPName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.imdb.com/title/"+episode.imdbId+"/?ref_=tt_cl_i1");
        }

        bool isScrolling = false;
        Point scrollMousePoint = new Point();
        double hOff = 1;
        private async void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            await Task.Run(() => {
                Thread.Sleep(100);
                isScrolling = true;
            });
            scrollMousePoint = e.GetPosition(ScrollView);
            hOff = ScrollView.HorizontalOffset;
            ScrollView.CaptureMouse();
        }

        private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (ScrollView.IsMouseCaptured) {
                ScrollView.ScrollToHorizontalOffset(hOff + (scrollMousePoint.X - e.GetPosition(ScrollView).X));
            }
        }

        private void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ScrollView.ReleaseMouseCapture();
        }

        private void BackIcon_MouseUp(object sender, MouseButtonEventArgs e) {

        }

        private async void Play_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            List<Episode.ScannedFile> list = new List<Episode.ScannedFile>();
            foreach (var item in episode.files) {
                if (item.Type == ScannedFile.FileType.Video) {
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
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    await Task.Run(() => {
                        Thread.Sleep(500);
                    });
                    MainWindow.AddPage(new LocalPlayer(Database.GetSeries((int)episode.seriesId), episode, sf));
                } else {
                    Process.Start(sf.NewName);
                }
            } else {
                await MessageBox.Show("No files found...","Error");
            }
        }

        private async void Download_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.AddPage(new PleaseWait());
            var ser = Database.GetSeries((int)episode.seriesId);
            Torrent torrent = await Torrent.SearchSingle(ser, episode, Settings.DownloadQuality);
            if (torrent != null) {
                TorrentDownloader downloader = new TorrentDownloader(torrent);
                await downloader.Download();
                NotificationSender se = new NotificationSender("Download started", Helper.GenerateName(Database.GetSeries((int)episode.seriesId), episode));
                se.ClickedEvent += (s, ev) => {
                    Dispatcher.Invoke(() => {
                        MainWindow.RemoveAllPages();
                        MainWindow.SetPage(new DownloadsView());
                    }, DispatcherPriority.Send);

                };
                se.Show();

            } else {
                await MessageBox.Show("Sorry, torrent not found");
            }
            MainWindow.RemovePage();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void Remove_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var ep = Database.GetEpisode((int)episode.seriesId, episode.id);
            var list = ep.files;
            foreach (var file in ep.files) {
                if (File.Exists(file.NewName)) {
                    File.Delete(file.NewName);              
                }
            }
            ep.files = new List<ScannedFile>();
            Database.EditEpisode((int)ep.seriesId, ep.id, ep);
        }

        private void EpisodeName_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void EpisodeName_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private async void Stream_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            MainWindow.AddPage(new PleaseWait());
            Torrent tor = await Torrent.SearchSingle(Database.GetSeries((int)episode.seriesId), episode, Settings.StreamQuality);
            MainWindow.RemovePage();
            TorrentDownloader td = new TorrentDownloader(tor);
            await td.Stream();
        }
    }
}
