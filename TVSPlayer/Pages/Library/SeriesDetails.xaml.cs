using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Task.Run(() => {
                Dispatcher.Invoke(() => {
                    genres.Text = "";
                    for (int i = 0; i < series.genre.Count; i++) {
                        if (i != series.genre.Count - 1) {
                            genres.Text += series.genre[i] + ", ";
                        } else {
                            genres.Text += series.genre[i];
                        }
                    }
                    showName.Text = series.seriesName;
                    schedule.Text = series.airsDayOfWeek + " at " + series.airsTime;
                    network.Text = series.network;
                    stat.Text = series.status;
                    prem.Text = series.firstAired;
                    len.Text = series.runtime;
                    summary.Text = series.overview;
                    agerating.Text = series.rating;
                    rating.Text = series.siteRating + "/10";

                },System.Windows.Threading.DispatcherPriority.Send);
               
            });

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
