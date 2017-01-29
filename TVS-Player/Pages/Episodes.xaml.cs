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
using System.Windows.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Episodes.xaml
    /// </summary>
    public partial class Episodes : Page {
        int id;
        int season;
        SelectedShows ss;
        public Episodes(SelectedShows s, int season2) {
            InitializeComponent();
            id = Int32.Parse(s.idSel);
            season = season2;
            ss = s;
        }

        private void List_Loaded(object sender, RoutedEventArgs e) {
            Action a;
            a = () => LoadEP();
            Thread thread = new Thread(a.Invoke);
            thread.Start();

        }
        private void LoadEP() {
            List<Episode> temp = new List<Episode>();
            List<Episode> tempCheck = DatabaseEpisodes.readDb(id);
            foreach (Episode episode in tempCheck) {
                if (episode.season == season) {
                    temp.Add(episode);
                }
            }
            List<Episode> episodes = temp.OrderBy(ep => ep.episode).ToList();
            foreach (Episode episode in episodes) {
                string text;
                var ffProbe = new NReco.VideoInfo.FFProbe();
                if (episode.locations.Count > 0) {
                    var videoInfo = ffProbe.GetMediaInfo(episode.locations[0]);
                    text = videoInfo.Duration.ToString(@"hh\:mm\:ss");
                } else {
                    text= "--:--:--";
                }
                Dispatcher.Invoke(new Action(() => {
                    EpisodeControl EPC = new EpisodeControl();
                    EPC.EPGrid.MouseLeftButtonUp += (s, ev) => PlayEP(episode.locations[0]);
                    EPC.EpisodeName.Text = episode.name;
                    EPC.noEp.Text = getEPOrder(episode);
                    EPC.timerText.Text = text;
                    List.Children.Add(EPC);
                }), DispatcherPriority.Send);

                }
      
        }

        private void PlayEP(string path) {
            Page showPage = new Player(path);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }

        private string getEPOrder(Episode e) {
            int season = e.season;
            int episode = e.episode;
            if(season < 10) {
                if (episode < 10) {
                    return "S0" + season + "E0" + episode;
                } else if (episode >= 10) {
                    return "S0" + season + "E" + episode;
                }
            } else if (season >= 10) {
                if (episode < 10) {
                    return "S" + season + "E0" + episode;
                } else if (episode >= 10) {
                    return "S" + season + "E" + episode;
                }
            }
            return null;
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Page showPage = new Seasons(ss);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }
    }
}
