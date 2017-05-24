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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for DBCreationProgress.xaml
    /// </summary>
    public partial class DBCreationProgress : Page {
        private List<TVShow> list;
        private int count;
        private int currentShows = 0;
        private int currentEpisodes = 0;
        public DBCreationProgress(List<TVShow> list) {
            InitializeComponent();
            this.list = list;
            count = list.Count;
        }


        private void GetShowsInfo() {       
            foreach (TVShow s in list) {
                Task t = new Task(() => {
                    s.GetInfo();
                });
                t.ContinueWith((add) => currentShows++);
                t.Start();
            }
            while (currentShows != count) { Thread.Sleep(100); };
            TVShow.CreateDatabase(list);

        }
        private void GetEpisodes() {
            Dispatcher.Invoke(new Action(() => {
                ProgBar.Value = 0;
                ProgText.Text = "Creating database for episodes";
            }));
            foreach (TVShow s in list) {
                Task t = new Task(() => {
                    List<Episode> le = Episode.getAllEP(s);
                    Episode.CreateDatabase(le, s);
                });
                t.ContinueWith((add) => currentEpisodes++);
                t.Start();
            }
            while (currentEpisodes != count) { Thread.Sleep(100); };
        }

        private void MovePBar() {
            while (currentShows != count || currentEpisodes != count) {
                if (currentShows == count) {
                    Dispatcher.Invoke(new Action(() => {
                        ProgBar.Value = currentEpisodes;
                        ProgCount.Text = currentEpisodes + "/" + count;
                    }));
                } else {
                    Dispatcher.Invoke(new Action(() => {
                        ProgBar.Value = currentShows;
                        ProgCount.Text = currentShows + "/" + count;
                    }));
                }
                Thread.Sleep(100);
            }
        }

        private void Finished() {
            Dispatcher.Invoke(new Action(() => {
                MainWindow.RemovePage();
            }));
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            ProgBar.Value = 0;
            ProgBar.Maximum = count;
            Action mainAction = () => GetShowsInfo();
            Task mainWorker = new Task(mainAction.Invoke);
            mainWorker.ContinueWith((secondWorker) => GetEpisodes());
            mainWorker.Start();
            Action aChecker = () => MovePBar();
            Task checker = new Task(aChecker.Invoke);
            checker.ContinueWith((fin) =>Finished());
            checker.Start();
        }
    }
}
