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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for EpisodeView.xaml
    /// </summary>
    public partial class EpisodeView : UserControl {
        public EpisodeView( Episode episode, bool hasBackground) {
            InitializeComponent();
            this.episode = episode;
            this.hasBackground = hasBackground;
        }
        Episode episode;
        bool hasBackground;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            EpisodeName.Text = episode.episodeName;
            EpisodeNumber.Text = "Episode: " + episode.airedEpisodeNumber;
        }
    }
}
