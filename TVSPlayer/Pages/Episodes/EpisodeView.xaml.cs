using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for EpisodeView.xaml
    /// </summary>
    public partial class EpisodeView : UserControl {
        public EpisodeView( Episode episode, bool hasBackground, SeriesEpisodes episodeView) {
            InitializeComponent();
            this.episode = episode;
            this.hasBackground = hasBackground;
            this.episodeView = episodeView;
        }
        Episode episode;
        bool hasBackground;
        SeriesEpisodes episodeView;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            EpisodeName.Text = episode.episodeName;
            if(EpisodeNumber.Text == "Sample")
                EpisodeNumber.Text = "Episode: " + episode.airedEpisodeNumber;
        }

        private void ThumbImage_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
            var sb = (Storyboard)FindResource("OpacityUp");
            sb.Begin(CoverGrid);
        }

        private void ThumbImage_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
            var sb = (Storyboard)FindResource("OpacityDown");
            sb.Begin(CoverGrid);
        }

        private void CoverGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            RightClickEvent(episodeView, episode);
        }

        public static void RightClickEvent(SeriesEpisodes episodeView,Episode episode) {
            var sb = (Storyboard)Application.Current.FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => {
                episodeView.DetailsGrid.Children.RemoveRange(0, episodeView.DetailsGrid.Children.Count);
                episodeView.DetailsGrid.Children.Add(new EpisodeDetails(episode));
                Storyboard up = (Storyboard)Application.Current.FindResource("OpacityUp");
                up.Begin(episodeView.DetailsGrid);
            };
            clone.Begin(episodeView.DetailsGrid);
            episodeView.ScrollView.ScrollToTop();
        }

       

    }
}
