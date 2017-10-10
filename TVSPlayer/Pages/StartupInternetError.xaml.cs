using System;
using System.Collections.Generic;
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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for StartupInternetError.xaml
    /// </summary>
    public partial class StartupInternetError : Page {
        public StartupInternetError() {
            InitializeComponent();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
        }

        private void Reload_MouseUp(object sender, MouseButtonEventArgs e) {
            Storyboard s = FindResource("Rotate") as Storyboard;
            Storyboard anim = s.Clone();
            anim.Begin(ReloadImage);         
            if (MainWindow.checkConnection()) {
                MainWindow.RemovePage();
            }
        }
    }
}
