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
using System.Windows.Media.Animation;
using System.IO;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        private void MenuHide_Click(object sender, RoutedEventArgs e) {
            ShowHideMenu("sbHideLeftMenu", btnLeftMenuHide, btnLeftMenuShow, panelMenu);
            hideOnClick.Visibility = Visibility.Hidden;
        }

        private void MenuShow_Click(object sender, RoutedEventArgs e) {
            ShowHideMenu("sbShowLeftMenu", btnLeftMenuHide, btnLeftMenuShow, panelMenu);
            hideOnClick.Visibility = Visibility.Visible;
        }

        private void ShowHideMenu(string Storyboard, Button btnHide, Button btnShow, StackPanel pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);
            if (Storyboard.Contains("Show")) {
                btnHide.Visibility = System.Windows.Visibility.Visible;
                btnShow.Visibility = System.Windows.Visibility.Hidden;
            } else if (Storyboard.Contains("Hide")) {
                btnHide.Visibility = System.Windows.Visibility.Hidden;
                btnShow.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void OpenLibrary(object sender, RoutedEventArgs e)
        {
            Library lib = new Library();
            lib.Show();
        }
        private void btnShowsShow_Click(object sender, RoutedEventArgs e) {
            Frame.Content = new Shows();
        }
        private void btnDownloadShow_Click(object sender, RoutedEventArgs e) {
            Frame.Content = new Download();
        }

        private void FrameLoaded_Handler(object sender, RoutedEventArgs e) {
            /*if (File.Exists()) {
                Frame.Content = new Shows();
            } else { }*/
        }
    }
}
