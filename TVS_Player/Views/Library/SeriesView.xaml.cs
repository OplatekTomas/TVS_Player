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
        Dictionary<int, List<Episode>> EpisodesSorted { get; set; } = new Dictionary<int, List<Episode>>();

        public SeriesView(Series series) {
            InitializeComponent();
            this.series = series;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            var episodes = await Episode.GetEpisodes(series.Id);
            foreach (var ep in episodes) {
                if (Helper.ParseDate(ep.FirstAired, out var result) && result < DateTime.Now) {
                    if (EpisodesSorted.ContainsKey((int)ep.AiredSeason)) {
                        EpisodesSorted[(int)ep.AiredSeason].Add(ep);
                    } else {
                        EpisodesSorted.Add((int)ep.AiredSeason, new List<Episode>() { ep });
                    }
                }
            }
            var selector = new SeasonSelector(EpisodesSorted.Keys.Max(), async (s, ev) => await RenderSeason((int)s));
            var notWatchedEp = episodes.FirstOrDefault(x => x.AiredSeason > 0 && !x.Watched);
            if (notWatchedEp == default) {
                selector.SelectSeason((int)notWatchedEp.AiredSeason);
            } else {
                selector.SelectSeason(1);
            }
            SeasonController.Children.Add(selector);
            PosterImage.Source = await Helper.GetImage(series.URL);
            resizeTimer.Elapsed += ResizingDone;
            EpisodePanel.SizeChanged += Page_SizeChanged;
        }

        private async Task RenderSeason(int season) {
            EpisodePanel.Children.Clear();
            if (EpisodesSorted.ContainsKey(season)) {
                var (width, height) = GetDimensions();
                foreach (var item in EpisodesSorted[season].OrderBy(x=>x.AiredEpisodeNumber)) {
                    EpisodePreview preview = new EpisodePreview {
                        Height = height,
                        Width = width
                    };
                    preview.EpisodeName.Text = item.EpisodeName;
                    preview.MouseLeftButtonUp += async (s, ev) => await PlayEpisode(item);
                    preview.EpisodeNumber.Text = "Episode: " + item.AiredEpisodeNumber;
                    preview.EpisodeThumbnail.Source = await Helper.GetImage(item.URL);
                    EpisodePanel.Children.Add(preview);
                    Animate.FadeIn(preview);
                }
            }
        }


        private async Task PlayEpisode(Episode ep) {
            var files = await ScannedFile.GetFiles(ep.Id);
            var test = files.FirstOrDefault(x => x.FileType == "Video" || x.FileType == 1.ToString());
            if (test != default) {
                var pi = new ProcessStartInfo() {
                    Arguments = "http://" + Settings.Default.ServerIp + ":" + Settings.Default.ServerPort + test.URL,
                    UseShellExecute = true,
                    FileName = "C:\\Program Files\\VideoLAN\\VLC\\vlc.exe",
                    Verb = "OPEN"
                };
                Process.Start(pi);
            }
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
            int numberOf = Convert.ToInt32(EpisodePanel.ActualWidth) / 350;
            for (int i = 350; i >= 150; i--) {
                if (numberOf < Convert.ToInt32(EpisodePanel.ActualWidth) / i) {
                    return (i,(i*0.5625)+4.5);
                }
            }
            return (0, 0);
        }
    }

}
