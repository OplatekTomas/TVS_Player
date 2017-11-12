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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for PleaseWait.xaml
    /// </summary>
    public partial class PleaseWait : Page {
        public PleaseWait() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Animate();
        }
        private void Animate() {
            Storyboard sb = (Storyboard)FindResource("Rotate");
            Storyboard temp = sb.Clone();
            temp.Duration = new TimeSpan(0, 0, 0, 1);
            temp.Completed += (s, e) => {
                RotateImage.RenderTransform = new RotateTransform(1);
                Animate();
            };
            temp.Begin(RotateImage);
        }
    }
}
