using System;
using System.Collections.Generic;
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
        public SeasonView(List<Episode> episodes,Series series) {
            InitializeComponent();
            this.episodes = episodes;
            this.series = series;
        }
        List<Episode> episodes;
        Series series;

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

        private void ScrollView_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                ScrollView.LineLeft();
                ScrollView.LineLeft();
                ScrollView.LineLeft();
                ScrollView.LineLeft();

            } else {
                ScrollView.LineRight();
                ScrollView.LineRight();
                ScrollView.LineRight();
                ScrollView.LineRight();

            }
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            await Task.Run(async() => {
                foreach (Episode ep in episodes) {
                    await Task.Run(async () => {
                        BitmapImage bmp = await Database.GetEpisodeThumbnail(series.id, ep.id);
                        Episode episode = Database.GetEpisode(series.id, ep.id, true);
                        Dispatcher.Invoke(() => {
                            EpisodeView epv = new EpisodeView(episode, true);
                            epv.Width = 230;
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

        private async Task<BitmapImage> LoadThumb(Episode episode) {
            if (!String.IsNullOrEmpty(episode.filename)) {
                return await Database.LoadImage(new Uri("https://www.thetvdb.com/banners/" + episode.filename));
            }
            return null;
        }

    }
}
