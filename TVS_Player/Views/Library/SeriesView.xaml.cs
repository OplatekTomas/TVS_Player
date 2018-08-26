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
using TVS_Player_Base;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for SeriesView.xaml
    /// </summary>
    public partial class SeriesView : Page {

        Series series;
        Dictionary<int, List<Episode>> EpisodesSorted { get; set; } = new Dictionary<int, List<Episode>>();

        public SeriesView(Series series) {
            InitializeComponent();
            this.series = series;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            var episodes = await Episode.GetEpisodes(series.Id);
            foreach (var ep in episodes) {
                if (Helper.ParseDate(ep.FirstAired, out var result) && result < DateTime.Now) {
                    if (EpisodesSorted.ContainsKey((int)ep.AiredSeason)) {
                        EpisodesSorted[(int)ep.AiredSeason].Add(ep);
                    } else {
                        EpisodesSorted.Add((int)ep.AiredSeason, new List<Episode>() { ep });
                    }
                }
            }
            var selector = new SeasonSelector(EpisodesSorted.Keys.Max(), (s, ev) => RenderSeason((int)s));
            var notWatchedEp = episodes.FirstOrDefault(x => x.AiredSeason > 0 && !x.Watched);
            if (notWatchedEp == default) {
                selector.SelectSeason((int)notWatchedEp.AiredSeason);
            } else {
                selector.SelectSeason(1);
            }
            SeasonController.Children.Add(selector);
            PosterImage.Source = await Helper.GetImage(series.URL);
        }

        private void RenderSeason(int season) {

        }
    }
}
