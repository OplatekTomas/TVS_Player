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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SeasonView.xaml
    /// </summary>
    public partial class SeasonView : UserControl {
        public SeasonView(List<Episode> episodes,Series series, SeriesEpisodes episodeView) {
            InitializeComponent();
            this.episodes = episodes;
            this.series = series;
            this.episodeView = episodeView;
        }
        List<Episode> episodes;
        Series series;
        SeriesEpisodes episodeView;
        bool isScrolling = false;

        private void ScrollView_SizeChanged(object sender, SizeChangedEventArgs e) {
            if ((ScrollView.ActualWidth + 130) > this.ActualWidth) {
                var sb = (Storyboard)FindResource("GoWide");
               // sb.Begin(RightArrow);
                //sb.Begin(LeftArrow);
            } else {
                var sb = (Storyboard)FindResource("GoAway");
               // sb.Begin(RightArrow);
               // sb.Begin(LeftArrow);
            }
        }

        private void RightArrow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            for (int i = 0; i < 15; i++) {
                ScrollView.LineLeft();
            }
        }

        private void LeftArrow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            for (int i = 0; i < 15; i++) {
                ScrollView.LineRight();
            }
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SetPage(new Library());
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            await Task.Run(async() => {
                foreach (Episode ep in episodes) {
                    await Task.Run(async () => {
                        BitmapImage bmp = await Database.GetEpisodeThumbnail(series.id, ep.id);
                        Episode episode = Database.GetEpisode(series.id, ep.id, true);
                        Dispatcher.Invoke(() => {
                            EpisodeView epv = new EpisodeView(episode, true, episodeView);
                            epv.Width = 230;
                            epv.CoverGrid.MouseLeftButtonUp += (s, ev) => CoverGridMouseUp(episode);
                            epv.Opacity = 0;
                            epv.ThumbImage.Source = bmp;
                            epv.Margin = new Thickness(5, 0, 10, 0);
                            Panel.Children.Add(epv);
                            Storyboard sb = (Storyboard)FindResource("OpacityUp");
                            sb.Begin(epv);
                        });
                    });
                    Thread.Sleep(25);
                }
            });
           
        }

        private void CoverGridMouseUp(Episode episode) {
            if (!isScrolling) {
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
                    Process.Start(info.FullName);
                }
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
