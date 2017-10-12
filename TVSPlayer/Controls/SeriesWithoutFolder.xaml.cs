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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SearchShowResult.xaml
    /// </summary>
    public partial class SearchShowResult : UserControl {
        public SearchShowResult(int id) {
            InitializeComponent();
            this.id = id;
        }
        int id;
        private void Detail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Storyboard open = FindResource("OpenDetails") as Storyboard;
            open.Completed += (se, ev) => { DetailsPart.Visibility = Visibility.Visible; };
            open.Begin(MainPart);
            int id = this.id;
            Action posterAction = () => GetPoster(id);
            Action informationAction = () => GetInfo(id);
            Task informationTask = new Task(informationAction);
            Task posterTask = new Task(posterAction);
            informationTask.Start();
            posterTask.Start();
        }

        private void GetPoster(int id) {
            List<Poster> list = Poster.GetPosters(id);
            if (list.Count > 0) {
                Poster poster = list[0];
                MessageBox.Show("poster");
            } else {

            }
        }
        private void GetInfo(int id) {
            Series series = Series.GetSeries(id);
            MessageBox.Show("show");

        }

        private void DetailsPart_MouseLeave(object sender, MouseEventArgs e) {
            DetailsPart.Visibility = Visibility.Hidden;
            Storyboard close = FindResource("CloseDetails") as Storyboard;
            close.Begin(MainPart);
        }
    }
}
