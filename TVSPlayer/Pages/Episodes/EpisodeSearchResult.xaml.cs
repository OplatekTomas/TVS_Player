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
    /// Interaction logic for EpisodeSearchResult.xaml
    /// </summary>
    public partial class EpisodeSearchResult : UserControl {
        public EpisodeSearchResult(Episode episode) {
            InitializeComponent();
            this.episode = episode;
        }
        Episode episode;

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            MainText.Text = episode.episodeName;
            EpisodeText.Text = "Episode: " + episode.airedEpisodeNumber;
            SeasonText.Text = "Season: " + episode.airedSeason;
        }

        private void Base_MouseEnter(object sender, MouseEventArgs e) {
            Base.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
        }

        private void Base_MouseLeave(object sender, MouseEventArgs e) {
            Base.Background = Brushes.Transparent;
        }
    }
}
