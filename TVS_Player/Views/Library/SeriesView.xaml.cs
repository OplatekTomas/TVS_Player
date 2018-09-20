using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
using TVS_Player.Properties;
using TVS_Player_Base;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for SeriesView.xaml
    /// </summary>
    public partial class SeriesView : Page {

        Series series;
        int startSeason = 0;
        Dictionary<int, List<Episode>> EpisodesSorted { get; set; } = new Dictionary<int, List<Episode>>();

        public SeriesView(Series series) {
            InitializeComponent();
            this.series = series;
        }

        public SeriesView(Series series, int season) {
            InitializeComponent();
            this.series = series;
            startSeason = season;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            var episodes = await Episode.GetEpisodes(series.Id);
            if (EpisodesSorted.Count == 0) {
                foreach (var ep in episodes) {                   
                    if (Helper.ParseDate(ep.FirstAired, out var result) && result < DateTime.Now) {
                        if (EpisodesSorted.ContainsKey((int)ep.AiredSeason)) {
                            EpisodesSorted[(int)ep.AiredSeason].Add(ep);
                        } else {
                            EpisodesSorted.Add((int)ep.AiredSeason, new List<Episode>() { ep });
                        }
                    }
                }
            }
            
            var selector = new SeasonSelector(EpisodesSorted.Keys.Max(), async (s, ev) => await RenderSeason((int)s));
            var notWatchedEp = episodes.FirstOrDefault(x => x.AiredSeason > 0 && !x.Finished);

            if (startSeason > 0) {
                selector.SelectSeason(startSeason);
            } else if (notWatchedEp == default) {
                selector.SelectSeason((int)notWatchedEp.AiredSeason);
            } else {
                selector.SelectSeason(1);
            }
            SeasonController.Children.Clear();
            SeasonController.Children.Add(selector);
            Background.Source = await Helper.GetImage((await Poster.GetBackground(series.Id)).URL);
            Animate.FadeIn(Background);
            resizeTimer.Elapsed += ResizingDone;
            EpisodePanel.SizeChanged += Page_SizeChanged;
            var nextEp = episodes.Where(x => x.AiredSeason > 0 && !string.IsNullOrEmpty(x.FirstAired) && Helper.ParseDate(x.FirstAired) > DateTime.Now).OrderBy(x=>x.FirstAired).FirstOrDefault();
            FillInText(nextEp);
        }

        private async Task RenderSeason(int season) {
            EpisodePanel.Children.Clear();
            if (EpisodesSorted.ContainsKey(season)) {
                var (width, height) = GetDimensions();
                foreach (var item in EpisodesSorted[season].OrderBy(x=>x.AiredEpisodeNumber)) {
                    if (IsLoaded) {
                        EpisodePreview preview = new EpisodePreview {
                            Height = height,
                            Width = width
                        };
                        preview.EpisodeName.Text = item.EpisodeName;
                        preview.MouseLeftButtonUp += (s, ev) => View.AddPage(new VideoPlayer(series, item));

                        preview.EpisodeNumber.Text = "Episode: " + item.AiredEpisodeNumber;
                        preview.EpisodeThumbnail.Source = await Helper.GetImage(item.URL);
                        EpisodePanel.Children.Add(preview);
                        Animate.FadeIn(preview);
                    }
                }
            }
        }

        private void FillInText(Episode nextEp) {
            SeriesName.Text = series.SeriesName;
            Genre.Text = "";
            Rating.Text = series.SiteRating + "/10";
            if (nextEp != null) {
                NextEp.Text = Helper.ParseDateToString(nextEp.FirstAired) + " (" + Helper.ParseDate(nextEp.FirstAired).ToString("dddd") + ") @ " + series.AirsTime;
            } else {
                NextEpGrid.Visibility = Visibility.Collapsed;
            }
            Length.Text = series.Runtime.Length == 1 ? series.Runtime + " h" : series.Runtime + " min" ;
            Network.Text = series.Network;
            Status.Text = series.Status;
            Imdb.MouseLeftButtonUp += (s,ev) => Process.Start("https://www.imdb.com/title/" + series.ImdbId);
            for (int i = 0; i < series.Genre.Count; i++) {
                Genre.Text += i == series.Genre.Count - 1 ? series.Genre[i] : series.Genre[i] + ", ";
            }
            Animate.FadeIn(MainText);

        }


        Timer resizeTimer = new Timer(200);

        void ResizingDone(object sender, ElapsedEventArgs e) {
            resizeTimer.Stop();
            ResizeElements();
        }

        private void ResizeElements() {
            Dispatcher.Invoke(() => {
                var (width, height) = GetDimensions();
                var heightAnimation = new DoubleAnimation(height, TimeSpan.FromMilliseconds(200));
                var widthAnimation = new DoubleAnimation(width, TimeSpan.FromMilliseconds(200));
                foreach (UIElement item in EpisodePanel.Children) {
                    item.BeginAnimation(WidthProperty, widthAnimation);
                    item.BeginAnimation(HeightProperty, heightAnimation);
                }
            });

        }


        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private (double width, double height) GetDimensions() {
            int numberOf = Convert.ToInt32(EpisodePanel.ActualWidth) / 365;
            for (int i = 365; i >= 165; i--) {
                if (numberOf < Convert.ToInt32(EpisodePanel.ActualWidth) / i) {
                    return (i,(i*0.5625)+4.5);
                }
            }
            return (0, 0);
        }

        private void Imdb_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Imdb_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }

}
