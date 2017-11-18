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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : Page {
        public DownloadsView() {
            InitializeComponent();
        }
        List<TorrentUserControl> userControls = new List<TorrentUserControl>();

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            ContentFrame.Content = new CurrentTorrents();
        }

        private void CurrentDownloads_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ContentFrame.Content = new CurrentTorrents();
            AllGrid.Visibility = FinishedGrid.Visibility = Visibility.Hidden;
            CurrentGrid.Visibility = Visibility.Visible;
        }

        private void FinishedDownloads_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ContentFrame.Content = new FinishedTorrents();
            AllGrid.Visibility = CurrentGrid.Visibility = Visibility.Hidden;
            FinishedGrid.Visibility = Visibility.Visible;

        }

        private void AllDownloads_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ContentFrame.Content = new AllTorrents();
            FinishedGrid.Visibility = CurrentGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Visible;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void CurrentDownloads_MouseEnter(object sender, MouseEventArgs e) {

        }
    }
}
