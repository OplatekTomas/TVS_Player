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
    /// Interaction logic for FinishedTorrents.xaml
    /// </summary>
    public partial class FinishedTorrents : Page {
        public FinishedTorrents() {
            InitializeComponent();
        }

        Dictionary<FinishedTorrentUserControl, Torrent> uielements = new Dictionary<FinishedTorrentUserControl, Torrent>();

        private void Panel_Loaded(object sender, RoutedEventArgs e) {
            Render();
        }

        private void Render() {
            Task.Run(() => {
                bool isLoaded = true;
                Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                while (isLoaded) {
                    var list = TorrentDatabase.Load().Where(x=>x.HasFinished).ToList();
                    if (list.Count > uielements.Count) {
                        var torrents = new List<Torrent>();
                        uielements.Values.ToList().ForEach(x => torrents.Add(x));
                        List<Torrent> changes = list.Where(x => !torrents.Contains(x)).ToList();
                        foreach (var item in changes) {
                            Dispatcher.Invoke(() => {
                                FinishedTorrentUserControl tcu = new FinishedTorrentUserControl(item);
                                tcu.Height = 75;
                                tcu.Opacity = 0;
                                tcu.Margin = new Thickness(10, 0, 0, 0);
                                Panel.Children.Add(tcu);
                                var sb = (Storyboard)FindResource("OpacityUp");
                                sb.Begin(tcu);
                                uielements.Add(tcu, item);
                            });
                        }
                    }                  
                    Thread.Sleep(2000);
                    Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                }
            });
        }
    }
}
