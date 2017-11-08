using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;
using static TVS.API.Episode;

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
            Task.Run(() => LoadSeasons());
            Task.Run(() => LoadInfo());
        }

        private async void LoadInfo() {
            BitmapImage bmp = await Database.GetSelectedPoster(series.id);
            List<Episode> list = Database.GetEpisodes(series.id);
            var nextEpisode = list.Where(ep => !String.IsNullOrEmpty(ep.firstAired) && DateTime.ParseExact(ep.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture) > DateTime.Now).OrderBy(e => e.firstAired).ToList().FirstOrDefault();
            int episodeCount, downloadedEpisodes, seasonsCount;
            episodeCount = downloadedEpisodes = seasonsCount = 0;
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
            Dispatcher.Invoke(() => {
                DefaultPoster.Source = bmp;
                if (nextEpisode != null) {
                    NextDate.Text = DateTime.ParseExact(nextEpisode.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd. MM. yyyy");
                } else {
                    NextDate.Text = "-";
                }
                if (!String.IsNullOrEmpty(series.firstAired)){
                    Premiered.Text = DateTime.ParseExact(series.firstAired,"yyyy-MM-dd",CultureInfo.InvariantCulture).ToString("dd. MM. yyyy");
                }
                genres.Text = "";
                for (int i = 0; i < series.genre.Count; i++) {
                    if (i != series.genre.Count - 1) {
                        genres.Text += series.genre[i] + ", ";
                    } else {
                        genres.Text += series.genre[i];
                    }
                }
                showName.Text = series.seriesName;
                Status.Text = series.status;
                Schedule.Text = series.airsDayOfWeek + " at " + series.airsTime;
                Rating.Text = series.siteRating + "/10";
                SeasonCount.Text = seasonsCount.ToString();
                EpisodeCount.Text = episodeCount.ToString();
                EpisodesOffline.Text = downloadedEpisodes.ToString();
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
        private void LoadSeasons() {
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
                    text.Foreground = (Brush)FindResource("TextColor");
                    text.Margin = new Thickness(0, 0, 0, 10);
                    text.Text = "Season " + (sorted.IndexOf(list) + 1);
                    SeasonView sv = new SeasonView(list,series,this);
                    //sv.ScrollView.PanningMode = PanningMode.HorizontalFirst;
                    sv.ScrollView.PreviewMouseWheel += (s, ev) => {
                        if (ev.Delta > 0) {
                            ScrollView.LineUp();
                            ScrollView.LineUp();
                            ScrollView.LineUp();
                        } else {
                            ScrollView.LineDown();
                            ScrollView.LineDown();
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

        private void showName_MouseUp(object sender, MouseButtonEventArgs e) {

        }

        public static ScannedFile GetFileToPlay(Episode episode, Series series) {
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
               return list.Where(x => x.NewName == info.FullName).FirstOrDefault();
            }
            return null;
        }

        private async void PlayNextEpisode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            List<Episode> episodes = Database.GetEpisodes(series.id);
            var ep = episodes.Where(x => !x.finised && x.airedSeason > 0 && x.files.Where(y => y.Type == Episode.ScannedFile.FileType.Video).ToList().Count > 0).OrderBy(x => x.airedSeason).ThenBy(x => x.airedEpisodeNumber).ToList().FirstOrDefault();
            MainWindow.SetPage(new BlankPage());
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            await Task.Run(() => {
                Thread.Sleep(500);
            });
            MainWindow.AddPage(new LocalPlayer(series, ep, GetFileToPlay(ep, series)));
        }
    }
}
