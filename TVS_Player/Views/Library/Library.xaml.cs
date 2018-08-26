using System;
using System.Collections.Generic;
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
using TVS_Player_Base;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page
    {
        public Library()
        {
            InitializeComponent();
        }

        Timer resizeTimer = new Timer(200);

        private async void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e) {
            var list = await Series.GetSeries();
            var (width, height) = GetDimensions();
            foreach (var series in list) {
                SeriesPoster sp = new SeriesPoster();
                sp.MouseEnter += (s, ev) => ItemMouseEnter();
                sp.MouseLeave += (s, ev) => ItemMouseLeave();
                sp.Height = height;
                sp.Width = width;
                sp.MouseLeftButtonUp += (s, ev) => View.SetPage(new SeriesView(series));
                sp.PosterImage.Source = await Helper.GetImage(series.URL);
                SeriesPanel.Children.Add(sp);
                Animate.FadeIn(sp);
            }
            SizeChanged += Page_SizeChanged;
            resizeTimer.Elapsed += new ElapsedEventHandler(ResizingDone);
        }

        private void ItemMouseEnter() {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void ItemMouseLeave() {
            Mouse.OverrideCursor = null;
        }

        void ResizingDone(object sender, ElapsedEventArgs e) {
            resizeTimer.Stop();
            ResizeElements();
        }

        private void ResizeElements() {
            Dispatcher.Invoke(() => {
                var (width, height) = GetDimensions();
                var heightAnimation = new DoubleAnimation(height, TimeSpan.FromMilliseconds(200));
                var widthAnimation = new DoubleAnimation(width, TimeSpan.FromMilliseconds(200));
                foreach (UIElement item in SeriesPanel.Children) {
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
            int numberOf = Convert.ToInt32(SeriesPanel.ActualWidth) / 225;
            for (int i = 225; i >= 175; i--) {
                if (numberOf < Convert.ToInt32(SeriesPanel.ActualWidth) / i) {
                    return (i, (i * 1.47) - 5);
                }
            }
            return (0, 0);
        }
    }
}
