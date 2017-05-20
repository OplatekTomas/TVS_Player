using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for Details.xaml
    /// </summary>
    public partial class Details : Page {
        TVShow show;
        public Details(TVShow tvshow){
            InitializeComponent();
            show = tvshow;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            Action a = () => show.GetInfoTVMaze();
            Task tsk = new Task(a);
            tsk.ContinueWith(t2 => SetInfo());
            tsk.Start();
            TranslateTransform t = new TranslateTransform(0, BackgroundGrid.ActualHeight);
            ContentGrid.RenderTransform = t;
            StartAnimation("SlideToMiddle", ContentGrid);
            StartAnimation("OpacityUp", BackgroundGrid);
        }
        
        public void SetInfo() {
            Action a = () => DownloadAndSetPoster();
            Task t = new Task(a);
            t.Start();
            Dispatcher.Invoke(new Action(() => {
                ShowName.Text = show.seriesName;
                ShowName.MouseUp += (s, e) => OpenIMDB();
                Rating.Text = show.rating;
                for (int i = 0; i < show.genre.Count; i++) {
                    if (i == show.genre.Count-1) { 
                        Genres.Text += show.genre[i];
                    } else {
                        Genres.Text += show.genre[i] + ", ";
                    }
                }
                Status.Text = show.status;
                FirstAir.Text = show.firstAired;
                ShowDetails.Text = show.overview;
                Network.Text = show.network;
                AirDate.Text = show.airsDayOfWeek + "/" + show.airsTime;
                Rating.Text = show.rating + "/10";
            }), DispatcherPriority.Send);
        }

        public void DownloadAndSetPoster() {          
            Dispatcher.Invoke(new Action(() => {
                Poster.Source = new BitmapImage(new Uri(show.poster));
            }), DispatcherPriority.Send);

        }
        public void OpenIMDB() {
            Process.Start("http://www.imdb.com/title/"+show.imdbId+"/?ref_=nv_sr_1");
        }

        private void StartAnimation(string storyboard, Grid pnl) {
            Storyboard sb = this.FindResource(storyboard) as Storyboard;
            sb.Begin(pnl);
        }

        private void BackgroundGrid_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
        }
    }
}
