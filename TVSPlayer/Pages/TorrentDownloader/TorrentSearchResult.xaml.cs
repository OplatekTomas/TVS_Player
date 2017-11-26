using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for TorrentSearchResult.xaml
    /// </summary>
    public partial class TorrentSearchResult : UserControl {
        public TorrentSearchResult(Torrent torrent) {
            InitializeComponent();
            this.torrent = torrent;
        }
        Torrent torrent;

        private void SeriesInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SetPage(new SeriesEpisodes(torrent.Series));
        }

        private void SeriesInfo_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void SeriesInfo_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private async void Download_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            TorrentDownloader td = new TorrentDownloader(torrent);
            MainWindow.AddPage(new PleaseWait());
            await td.Download();
            await Task.Run(() => Thread.Sleep(500));
            MainWindow.RemovePage();

        }

        private void Question_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Process.Start(torrent.URL);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            TorrentName.Text =  torrent.Name;
            EpisodeInfo.Text = Helper.GenerateName(torrent.Episode) + " - " + torrent.Episode.episodeName;
            SeriesInfo.Text = "Series name: " + torrent.Series.seriesName;
            Quality.Text = "Quality: " + torrent.Quality.ToString();
            Size.Text = "Size: " + torrent.Size;
            SeedLeech.Text = "S: " + torrent.Seeders + " L: " + torrent.Leech; 
        }

        private void Stream_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }
    }
}
