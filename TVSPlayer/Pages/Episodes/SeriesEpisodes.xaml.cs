using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public Series series;

        bool hasBackground = false;

        Dictionary<Episode, string> searchValues = new Dictionary<Episode, string>();

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).SetLibrary();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization pg = new PageCustomization();
            pg.Buttons = new EpisodeButtons(this);
            pg.SearchBarEvent += async (s, ev) => { Search(); };
            pg.MainTitle = series.seriesName;
            MainWindow.SetPageCustomization(pg);
            Task.Run(() => LoadBackground());
            Task.Run(() => LoadSeasons());
            Task.Run(() => LoadInfo());
        }

        bool isRunning = false;
        Task searchTask;
        private async void Search() {
            SearchResultPanel.Children.Clear();
            string text = MainWindow.GetSearchBarText().ToLower();
            if (searchTask != null && searchTask.Status != TaskStatus.RanToCompletion) {
                isRunning = true;
                await Task.Run(() => {
                    Thread.Sleep(10);
                });
            }
            isRunning = false;
            searchTask = Task.Run( async() => {
                text = text.Trim();
                if (!String.IsNullOrEmpty(text)) {
                    Dispatcher.Invoke(() => {
                        SearchResultPanel.Visibility = Visibility.Visible;
                        SecondPanel.Visibility = Visibility.Collapsed;
                    });
                    var eps = GoThroughValues(text);
                    List<UIElement> list = new List<UIElement>();
                    foreach (var ep in eps) {
                        if (isRunning) {
                            list = new List<UIElement>();
                            Dispatcher.Invoke(() => {
                                SearchResultPanel.Children.Clear();
                            }, DispatcherPriority.Send);
                            break;
                        }
                        Episode episode = Database.GetEpisode(series.id, ep.id, true);
                        Dispatcher.Invoke(() => {
                            EpisodeSearchResult esr = new EpisodeSearchResult(episode);
                            esr.PlayIcon.PreviewMouseLeftButtonUp += (s, ev) => SearchClickEvent(episode);
                            esr.Clickable.PreviewMouseUp += (s, ev) => SearchClickEvent(episode);
                            esr.QuestionIcon.PreviewMouseUp += (s, ev) => EpisodeView.RightClickEvent(this, episode);
                            esr.Height = 60;
                            SearchResultPanel.Children.Add(esr);
                        }, DispatcherPriority.Send);
                    }
                } else {
                    Dispatcher.Invoke(() => {
                        SearchResultPanel.Visibility = Visibility.Collapsed;
                        SecondPanel.Visibility = Visibility.Visible;
                    });
                }
            });    
        }

        private async void SearchClickEvent(Episode episode) {
            if (!(await SeasonView.EpisodeViewMouseLeftUp(series, episode))) {
                EpisodeView.RightClickEvent(this, episode);
            }
        }

        private List<Episode> GoThroughValues(string text) {
            List<Episode> eps = searchValues.Keys.ToList();
            var result = new List<Episode>();
            Match seasonRgx = new Regex("s[0-9]?[0-9]", RegexOptions.IgnoreCase).Match(text);
            Match episodeRgx = new Regex("e[0-9]?[0-9]", RegexOptions.IgnoreCase).Match(text);
            if (seasonRgx.Success) {
                result = eps.Where(x => GetSeasonsOrEpisodes(seasonRgx.Value).Contains((int)x.airedSeason)).ToList();
                eps = result.Count > 0 ? result : eps;
                text = text.Replace(seasonRgx.Value, "");
            }
            if (episodeRgx.Success) {
                result = eps.Where(x => GetSeasonsOrEpisodes(episodeRgx.Value).Contains((int)x.airedEpisodeNumber)).ToList();
                eps = result.Count > 0 ? result : eps;
                text = text.Replace(episodeRgx.Value, "");
            }
            //Removes whitespace at start and end. Replaces all other whitespaces with single space
            text = text.Trim();
            text = Regex.Replace(text, @"\s+", " ");
            eps = eps.Where(x => x.episodeName.ToLower().Contains(text)).ToList();
            return eps;
        }

        private List<int> GetSeasonsOrEpisodes(string text) {
            List<int> seasons = new List<int>();
            int season = Int32.Parse(text.Remove(0, 1));
            if (text.Length >= 3) {
                return new List<int>() { season };
            }
            if (season == 0) {
                return new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
            for (int i = 0; i < 100; i++) {
                if (i.ToString()[0] == season.ToString()[0]) {
                    seasons.Add(i);
                }
            }
            return seasons;
        }

        private void GenerateSearch(List<Episode> episodes) {
            episodes = episodes.Where(x => x.airedSeason.ToString() != "0" && !String.IsNullOrEmpty(x.airedSeason.ToString()) && !String.IsNullOrEmpty(x.firstAired) && DateTime.ParseExact(x.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1) < DateTime.Now).ToList();
            foreach (var episode in episodes) {
                searchValues.Add(episode, episode.episodeName);
            }
        }

        private async void LoadInfo() {
            BitmapImage bmp = await Database.GetSelectedPoster(series.id);
            List<Episode> list = Database.GetEpisodes(series.id);
            GenerateSearch(list);
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
                    NextDate.Text = DateTime.ParseExact(nextEpisode.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1).ToString("dd. MM. yyyy");
                } else {
                    NextDate.Text = "-";
                }
                if (!String.IsNullOrEmpty(series.firstAired)) {
                    Premiered.Text = DateTime.ParseExact(series.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd. MM. yyyy");
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
            }, DispatcherPriority.Send);
        }

        public void LoadSeasons() {
            List<Episode> eps = Database.GetEpisodes(series.id);
            List<List<Episode>> sorted = new List<List<Episode>>();
            for (int i = 1; ; i++) {
                List<Episode> list = eps.Where(a => a.airedSeason == i && !String.IsNullOrEmpty(a.firstAired) && DateTime.ParseExact(a.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1) < DateTime.Now).ToList();
                if (Properties.Settings.Default.EpisodeSort) list.Reverse();
                if (list.Count != 0) {
                    sorted.Add(list);
                } else {
                    break;
                }
            }
            if (Properties.Settings.Default.EpisodeSort) sorted.Reverse();
            Dispatcher.Invoke(() => {
                SecondPanel.Children.RemoveRange(0, SecondPanel.Children.Count);
                foreach (var list in sorted) {
                    SeasonView sv = new SeasonView(list, series, this);
                    sv.ScrollView.PreviewMouseWheel += (s, ev) => Scroll(ev);
                    sv.Height = 195;
                    sv.Margin = new Thickness(0, 0, 35, 20);
                    SecondPanel.Children.Add(GenerateText("Season " + list[0].airedSeason));
                    SecondPanel.Children.Add(sv);
                }
            }, DispatcherPriority.Send);
        }

        private TextBlock GenerateText(string textblockText) {
            TextBlock text = new TextBlock();
            text.FontSize = 24;
            text.Foreground = (Brush)FindResource("TextColor");
            text.Margin = new Thickness(0, 0, 0, 10);
            text.Text = textblockText;
            return text;
        }

        private void Scroll(MouseWheelEventArgs ev) {
            if (ev.Delta > 0) {
                ScrollView.LineUp();
                ScrollView.LineUp();
                ScrollView.LineUp();
            } else {
                ScrollView.LineDown();
                ScrollView.LineDown();
                ScrollView.LineDown();
            }
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
            if (ep != null) { 
            var sf = GetFileToPlay(ep, series);
                if (!Settings.UseWinDefaultPlayer) {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    await Task.Run(() => {
                        Thread.Sleep(500);
                    });
                    MainWindow.AddPage(new LocalPlayer(series, ep, sf));
                } else {
                    Process.Start(sf.NewName);
                }
            }
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
