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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for FinishedTorrentUserControl.xaml
    /// </summary>
    public partial class FinishedTorrentUserControl : UserControl {
        public FinishedTorrentUserControl(Torrent torrent) {
            InitializeComponent();
            this.torrent = torrent;
        }
        Torrent torrent;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            TorrentName.Text = "Torrent name: " +  torrent.Name;
            EpisodeInfo.Text = Helper.GenerateName(torrent.Episode) + " - " + torrent.Episode.episodeName;
            SeriesInfo.Text = "Series name: " + torrent.Series.seriesName;
            Quality.Text = "Quality: " +  torrent.Quality.ToString();
            FinishedAt.Text = "Finished: " + torrent.FinishedAt;
            Size.Text = "Size: " + torrent.Size;
        }

        private void Remove_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            this.Visibility = Visibility.Collapsed;
            TorrentDatabase.Remove(torrent.Magnet);
        }

        private async void Play_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            bool playing = await SeasonView.EpisodeViewMouseLeftUp(torrent.Series, torrent.Episode);
            if (!playing) {
                await MessageBox.Show("Files were probably deleted","Error");
            }
        }

        private void SeriesInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SetPage(new SeriesEpisodes(torrent.Series));
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
