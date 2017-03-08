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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for SelectTorrent.xaml
    /// </summary>
    public partial class SelectTorrent : Page {
        public SelectTorrent(List<TorrentItem> torrents,Show show, Episode episode) {
            InitializeComponent();
            this.torrents = torrents;
            this.show = show;
            this.episode = episode;
            Load();
        }
        List<TorrentItem> torrents;
        Show show;
        Episode episode;

        private void Load() {
            Top.Text = episode.name;
            foreach (TorrentItem torrent in torrents) {
                TorrentControl tc = new TorrentControl();
                tc.tName.Text = torrent.name;
                tc.tSeed.Text = "Se: " + torrent.seeders;
                tc.tLeech.Text = "Le: " + torrent.leech;
                tc.tQual.Text = torrent.quality;
                tc.tSize.Text = torrent.size;
                tc.downloadThis.Click += (s, e) => DownloadClick(torrent);
                panel.Children.Add(tc);
            }
        }

        private void qualityswitch_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void DownloadClick(TorrentItem t) {
            TorrentDownloader td = new TorrentDownloader(t, episode, show);
            td.DownloadTorrent();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
        }

    }
}
