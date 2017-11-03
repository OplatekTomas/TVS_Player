using System;
using System.Collections.Generic;
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
using TVS.API;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for SeriesInLibrary.xaml
    /// </summary>
    public partial class SeriesInLibrary : UserControl
    {
        public SeriesInLibrary(Series series)
        {
            InitializeComponent();
            this.series = series;
        }
        public Series series;
        bool showing = false;

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            PosterImage.ToolTip = series.seriesName;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {

        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            if (!showing) {
                Show();
                showing = true;
            } else {
                Hide();
                showing = false;
            }
        }

        private void Show() {
            DoubleAnimation animation = new DoubleAnimation(this.ActualHeight / 8, new TimeSpan(0, 0, 0, 0, 200));
            animation.AccelerationRatio = animation.DecelerationRatio = 0.5;
            DetailsGrid.BeginAnimation(HeightProperty, animation);
            RemoveIcon.Height = this.ActualHeight / 16;
            PosterIcon.Height = this.ActualHeight / 14;
            PosterIcon.Margin = new Thickness(0, 0, this.ActualWidth / 7.5, 3);
            QuestionIcon.Height = this.ActualHeight / 15;
            QuestionIcon.Margin = new Thickness(0, 0, this.ActualWidth / 3.75, 0);
            //ShowName.FontSize = this.ActualHeight / 14;
            Storyboard sb = (Storyboard)FindResource("OpacityUp");
            sb.Begin(DetailsGrid);
        }

        private void Hide() {
            DoubleAnimation animation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 0, 200));
            animation.AccelerationRatio = animation.DecelerationRatio = 0.5;
            DetailsGrid.BeginAnimation(HeightProperty, animation);
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            sb.Begin(DetailsGrid);
        }

    }
}
