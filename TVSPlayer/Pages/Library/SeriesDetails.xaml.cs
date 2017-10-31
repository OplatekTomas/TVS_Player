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
        public SeriesDetails(Series series)
        {
            InitializeComponent();
            this.series = series;
        }
        Series series;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            List<Actor> actors = Database.GetActors(series.id);
            Task.Run(() => {
                foreach (Actor actor in actors) {
                    Dispatcher.Invoke(() => {
                        var auc = new ActorUserControl(actor);
                        auc.Name.Text = actor.name;
                        auc.Name.MouseUp += (s, ev) => { Process.Start("http://www.imdb.com/find?ref_=nv_sr_fn&q="+actor.name.Replace(' ','+')+"&s=all"); };
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
            } else { 
                ScrollView.LineRight();
                ScrollView.LineRight();
            }
        }
    }
}
