using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for Seasons.xaml
    /// </summary>
    public partial class Seasons : Page {
        private SelectedShows selectedShow;
        public Seasons(SelectedShows ss) {
            InitializeComponent();
            selectedShow = ss;
            StartUp();
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Page showPage = new Shows();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }

        private void StartUp() {
            int seasons = DatabaseAPI.GetSeasons(Int32.Parse(selectedShow.idSel));
            for (int i = 1; i <= seasons; i++) {
                SeasonControl se = CreateControl(i);
                List.Children.Add(se);
            }
            string path = Helpers.path + selectedShow.idSel + "\\" + selectedShow.posterFilename;
            if (selectedShow.posterFilename == null) {
                path += "\\" + selectedShow.idSel + ".jpg";
            }
            Poster.Source = new BitmapImage(new Uri(path));
        }
        private void ShowSeason(int season) {
            Page ep = new Episodes(selectedShow,season);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(ep);
        }
        private SeasonControl CreateControl(int season) {
            SeasonControl se = new SeasonControl();
            se.seasonText.Text = "Season " + season;
            se.noEp.Text = DatabaseEpisodes.GetEpPerSeason(Int32.Parse(selectedShow.idSel), season).ToString();
            se.finishedText.Text = "No";
            se.SeasonGrid.MouseDown += (s, e) => { ShowSeason(season); };
            se.Height = 45;
            return se;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}
