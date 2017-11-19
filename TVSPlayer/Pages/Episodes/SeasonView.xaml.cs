using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;
using static TVS.API.Episode;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SeasonView.xaml
    /// </summary>
    public partial class SeasonView : UserControl {
        public SeasonView(List<Episode> episodes,Series series, SeriesEpisodes episodeView) {
            InitializeComponent();
            this.episodes = episodes;
            this.series = series;
            this.seriesEpisodes = episodeView;
        }
        List<Episode> episodes;
        Series series;
        SeriesEpisodes seriesEpisodes;
        bool isScrolling = false;

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SetPage(new Library());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Task.Run(async () => {
                foreach (Episode ep in episodes) {
                    Episode episode = Database.GetEpisode(series.id, ep.id, true);
                    BitmapImage bmp = await Database.GetEpisodeThumbnail(series.id, ep.id);
                    Dispatcher.Invoke(() => {
                        EpisodeView epv = new EpisodeView(episode, true, seriesEpisodes);
                        epv.Width = 230;
                        epv.CoverGrid.MouseLeftButtonUp += (s, ev) => CoverGridMouseUp(episode);
                        epv.Opacity = 0;
                        epv.ThumbImage.Source = bmp;
                        epv.Margin = new Thickness(5, 0, 10, 0);
                        Panel.Children.Add(epv);
                        Storyboard sb = (Storyboard)FindResource("OpacityUp");
                        sb.Begin(epv);
                    }, DispatcherPriority.Send);
                }
            });
        }

        private async void CoverGridMouseUp(Episode episode) {
            if (!isScrolling && !(await EpisodeViewMouseLeftUp(series, episode))) {
                EpisodeView.RightClickEvent(seriesEpisodes, episode);
            }         
        }

        public async static Task<bool> EpisodeViewMouseLeftUp(Series series,Episode episode) {
            List<Episode.ScannedFile> list = new List<Episode.ScannedFile>();
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
                //Used to release as many resources as possible to give all rendering power to video playback
                MainWindow.SetPage(new BlankPage());
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                await Task.Run(() => {
                    Thread.Sleep(500);
                });
                MainWindow.AddPage(new LocalPlayer(series, episode, sf));
                return true;
            } else {
                return false;
            }
        }

        private async Task<BitmapImage> LoadThumb(Episode episode) {
            if (!String.IsNullOrEmpty(episode.filename)) {
                return await Database.LoadImage(new Uri("https://www.thetvdb.com/banners/" + episode.filename));
            }
            return null;
        }


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

        private async void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ScrollView.ReleaseMouseCapture();
            await Task.Run(() => {
                Thread.Sleep(500);
                isScrolling = false;
            });
        }

    }
}
