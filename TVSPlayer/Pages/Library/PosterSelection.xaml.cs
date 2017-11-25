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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for PosterSelection.xaml
    /// </summary>
    public partial class PosterSelection : UserControl {
        public PosterSelection(Poster poster, BitmapImage bmp) {
            InitializeComponent();
            this.bmp = bmp;
            this.poster = poster;
        }
        private BitmapImage bmp;
        public Poster poster;
        public bool selected = false;

        public void Background_MouseUp(object sender, MouseButtonEventArgs e) {
            if (selected) {
                Background.Background = (Brush)FindResource("BackgroundBrush");
                selected = false;
            } else {
                selected = true;
                Background.Background = (Brush)FindResource("AccentColor");
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PosterImage.Source = bmp;
        }

        private void Background_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Background_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
