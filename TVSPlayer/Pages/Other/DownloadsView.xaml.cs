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
            Task.Run(() => {
                foreach (var item in TorrentDownloader.torrents) {
                    Dispatcher.Invoke(() => {
                        TorrentUserControl tcu = new TorrentUserControl(item);
                        tcu.Height = 75;
                        tcu.Margin = new Thickness(10, 0, 0, 0);
                        Panel.Children.Add(tcu);
                    });
                    Thread.Sleep(16);

                }
            });
        }
    }
}
