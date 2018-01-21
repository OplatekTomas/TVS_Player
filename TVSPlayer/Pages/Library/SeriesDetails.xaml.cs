using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for SeriesDetails.xaml
    /// </summary>
    public partial class SeriesDetails : Page
    {
        public SeriesDetails(Series series, Page WhereToBack)
        {
            InitializeComponent();
            this.series = series;
            this.page = WhereToBack;
        }
        Series series;
        Page page;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization pg = new PageCustomization();
            pg.MainTitle = series.seriesName +  " - Details";
            MainWindow.SetPageCustomization(pg);
            LoadBackground();
            Task.Run(() => LoadInfo());
            List<Actor> actors = Database.GetActors(series.id);
            Task.Run(() => {
                foreach (Actor actor in actors) {
                    Dispatcher.Invoke(() => {
                        var auc = new ActorUserControl(actor);
                        auc.Name.Text = actor.name;
                        auc.Name.MouseUp += (s, ev) => { Process.Start("http://www.imdb.com/find?ref_=nv_sr_fn&q=" + actor.name.Replace(' ', '+') + "&s=all"); };
                        auc.Role.Text = actor.role;
                        auc.Opacity = 0;
                        auc.Margin = new Thickness(0, 0, 20, 5);
                        Panel.Children.Add(auc);
                        var sb = (Storyboard)FindResource("OpacityUp");
                        sb.Begin(auc);
                    });
                    Thread.Sleep(16);
                }

            });
            Task.Run( async() => {
                var bmp = await Database.GetSelectedPoster(series.id);
                Dispatcher.Invoke(() => {
                    PosterImage.Opacity = 0;
                    PosterImage.Source = bmp;
                    var sb = (Storyboard)FindResource("OpacityUp");
                    sb.Begin(PosterImage);
                });
            });


        }

        private void LoadInfo() {
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
                if (nextEpisode != null) {
                    NextDate.Text = DateTime.ParseExact(nextEpisode.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1).ToString("dd. MM. yyyy");
                } else {
                    NextDate.Text = "-";
                }
                if (!String.IsNullOrEmpty(series.firstAired)) {
                    Prem.Text = DateTime.ParseExact(series.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd. MM. yyyy");
                }
                Genres.Text = "";
                for (int i = 0; i < series.genre.Count; i++) {
                    if (i != series.genre.Count - 1) {
                        Genres.Text += series.genre[i] + ", ";
                    } else {
                        Genres.Text += series.genre[i];
                    }
                }
                ShowName.Text = series.seriesName;
                Schedule.Text = series.airsDayOfWeek + " at " + series.airsTime;
                Network.Text = series.network;
                Stat.Text = series.status;
                Len.Text = series.runtime;
                Summary.Text = series.overview;
                Agerating.Text = series.rating;
                Rating.Text = series.siteRating + "/10";
                SeasonCount.Text = seasonsCount.ToString();
                EpisodeCount.Text = episodeCount.ToString();
                EpisodesOffline.Text = downloadedEpisodes.ToString();
            });

        }

        private async void LoadBackground() {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if (!Settings.PerformanceMode) { 
                Task.Run( async () => {
                    BitmapImage bmp = await Database.GetFanArt(series.id);
                    Dispatcher.Invoke(() => {
                        if (bmp != null) {
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
                });
            }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        }

        private void ScrollView_SizeChanged(object sender, SizeChangedEventArgs e) {
            if ((ScrollView.ActualWidth + 130) > this.ActualWidth) {
                var sb = (Storyboard)FindResource("GoWide");
                sb.Begin(RightArrow);
                sb.Begin(LeftArrow);
            } else {
                var sb = (Storyboard)FindResource("GoAway");
                sb.Begin(RightArrow);
                sb.Begin(LeftArrow);
            }
            Column.Width = new GridLength(BackgroundGrid.ActualHeight*0.85);

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
            MainWindow.SetPage(page);
        }

        private void ScrollView_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                ScrollView.LineLeft();
                ScrollView.LineLeft();
            } else { 
                ScrollView.LineRight();
                ScrollView.LineRight();
            }
        }

        private void showName_MouseUp(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.imdb.com/title/" + series.imdbId + "/?ref_=fn_al_tt_1");
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        Point scrollMousePoint = new Point();
        double hOff = 1;
        private void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
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
    }
}
