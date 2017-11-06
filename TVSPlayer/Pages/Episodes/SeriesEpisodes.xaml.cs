using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SeriesEpisodes.xaml
    /// </summary>
    public partial class SeriesEpisodes : Page {
        public SeriesEpisodes(Series series) {
            InitializeComponent();
            this.series = series;
        }
        Series series;
        
        bool hasBackground = false;


        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SetPage(new Library()); 
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization pg = new PageCustomization();
            pg.MainTitle = series.seriesName;
            MainWindow.SetPageCustomization(pg);
            Task.Run(() => LoadBackground());
            Task.Run(() => LoadInfo());
            Task.Run(() => LoadSeasons());
        }

        private async void LoadInfo() {
            BitmapImage bmp = await Database.GetSelectedPoster(series.id);
            List<Episode> list = Database.GetEpisodes(series.id);
            int episodeCount, downloadedEpisodes, seasonsCount, missingEpisodes;
            episodeCount = downloadedEpisodes = seasonsCount = missingEpisodes = 0;
            foreach (Episode ep in list) {
                if (ep.airedSeason != 0) {
                    episodeCount++;
                }
                if (ep.airedSeason > seasonsCount) {
                    seasonsCount++;
                }
                if (ep.files.Count > 0) {
                    downloadedEpisodes++;
                }
            }
            missingEpisodes = episodeCount - downloadedEpisodes;
            var nextEpisode = list.Where(ep => !String.IsNullOrEmpty(ep.firstAired) && DateTime.ParseExact(ep.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture) > DateTime.Now).OrderBy(e => e.firstAired).ToList().FirstOrDefault();

            Dispatcher.Invoke(() => {
                DefaultPoster.Source = bmp;
                
            });

        }


        private async void LoadBackground() {
            BitmapImage bmp = await Database.GetFanArt(series.id);
            Dispatcher.Invoke(() => {
                if (bmp != null) {
                    hasBackground = true;
                    BackgroundImage.Source = bmp;
                    Darkener.Visibility = Visibility.Visible;
                    var sb = (Storyboard)FindResource("BlurImage");
                    sb.Begin();
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(BackgroundImage);
                } else {
                    BackButton.SetResourceReference(Image.SourceProperty, "BackIcon");
                }
            },DispatcherPriority.Send);          
        }
        private async void LoadSeasons() {
            List<Episode> eps = Database.GetEpisodes(series.id);
            List<List<Episode>> sorted = new List<List<Episode>>();

            for (int i = 1; ; i++) {
                List<Episode> list = eps.Where(a => a.airedSeason == i && !String.IsNullOrEmpty(a.firstAired) && DateTime.ParseExact(a.firstAired,"yyyy-MM-dd",CultureInfo.InvariantCulture) < DateTime.Now).ToList();
                if (list.Count != 0) {
                    sorted.Add(list);
                } else {
                    break;
                }
            }
            Dispatcher.Invoke(() => {
                foreach (var list in sorted) {
                    TextBlock text = new TextBlock();
                    text.FontSize = 24;
                    text.FontWeight = FontWeights.Bold;
                    text.Foreground = (Brush)FindResource("TextColor");
                    text.Margin = new Thickness(0, 0, 0, 10);
                    text.Text = "Season " + (sorted.IndexOf(list) + 1);
                    SeasonView sv = new SeasonView(list,series);
                    //sv.ScrollView.PanningMode = PanningMode.HorizontalFirst;
                    sv.ScrollView.PreviewMouseWheel += (s, ev) => {
                        if (ev.Delta > 0) {
                            ScrollView.LineUp();
                        } else {
                            ScrollView.LineDown();
                        }
                    };
                    sv.Height = 195;
                    sv.Margin = new Thickness(0, 0, 25, 20);
                    Panel.Children.Add(text);
                    Panel.Children.Add(sv);
                }
            }, DispatcherPriority.Send);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) {

        }
    }
}
