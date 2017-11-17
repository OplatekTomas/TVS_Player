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
            var sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => {
                if (episodeView.DetailsGrid.Children.Count == 2) {
                    episodeView.DetailsGrid.Children.RemoveAt(1);
                }
                EpisodeDetails epDetails = new EpisodeDetails(episode);
                epDetails.Opacity = 0;
                epDetails.Height = 400;
                epDetails.BackIcon.MouseUp += (se, eve) => Remove(epDetails); 
                epDetails.ScrollView.PreviewMouseWheel += (se, eve) => {
                    if (eve.Delta > 0) {
                        episodeView.ScrollView.LineUp();
                        episodeView.ScrollView.LineUp();
                        episodeView.ScrollView.LineUp();
                    } else {
                        episodeView.ScrollView.LineDown();
                        episodeView.ScrollView.LineDown();
                        episodeView.ScrollView.LineDown();
                    }
                };
                episodeView.DetailsGrid.Children.Add(epDetails);
                var sboard = (Storyboard)FindResource("OpacityUp");
                sboard.Begin(epDetails);
            };
            if (episodeView.DetailsGrid.Children.Count == 1) {
                clone.Begin(episodeView.SeriesDetails);
            } else {
                clone.Begin((FrameworkElement)episodeView.DetailsGrid.Children[1]);
            }
            episodeView.ScrollView.ScrollToTop();

        }

        private void Remove(EpisodeDetails details) {
            var sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => {
                episodeView.DetailsGrid.Children.Remove(details);
                var sboard = (Storyboard)FindResource("OpacityUp");
                sboard.Begin(episodeView.SeriesDetails);
            };
            clone.Begin(details);
        }

    }
}
