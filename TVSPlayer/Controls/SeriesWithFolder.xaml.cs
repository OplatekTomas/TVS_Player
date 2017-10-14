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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SeriesWithFolder.xaml
    /// </summary>
    public partial class SeriesWithFolder : UserControl {
        public SeriesWithFolder(int id) {
            InitializeComponent();
            this.id = id;
        }
        public int id;
        bool hasBeenUpdated = false;
        Series series;
        BitmapImage poster;
        bool showingDetails = false;
        private void Detail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!showingDetails) {
                Storyboard open = FindResource("OpenDetails") as Storyboard;
                open.Completed += (se, ev) => { DetailsPart.Visibility = Visibility.Visible; };
                open.Begin(MainPart);
                showingDetails = true;
                StartTasks();
                BGGrid.Background = (Brush)FindResource("LighterBG");
            } else {
                DetailsPart.Visibility = Visibility.Hidden;
                posterImage.Opacity = 0;
                LoadingText.Opacity = 1;
                DataPart.Opacity = 0;
                Storyboard close = FindResource("CloseDetails") as Storyboard;
                close.Begin(MainPart);
                showingDetails = false;
                BGGrid.Background = new BrushConverter().ConvertFromString("#00000000") as SolidColorBrush;
            }
        }

        /// <summary>
        /// Forces redraw of details if details have already been drawn at least once
        /// </summary>
        public void ForceRedraw() {
            if (hasBeenUpdated) {
                hasBeenUpdated = false;
                series = null;
                poster = null;
                StartTasks();
            }

        }

        private void StartTasks() {
            int id = this.id;
            Action posterAction = () => GetPoster(id);
            Action informationAction = () => GetInfo(id);
            Task informationTask = new Task(informationAction);
            Task posterTask = new Task(posterAction);
            informationTask.Start();
            posterTask.Start();
        }

        private void GetPoster(int id) {
            if (poster == null) {
                List<Poster> list = Poster.GetPosters(id);
                if (list.Count > 0) {
                    Poster poster = list[0];
                    Dispatcher.Invoke(new Action(() => {
                         this.poster = new BitmapImage(new Uri(Helper.posterLink + poster.fileName));
                    }), DispatcherPriority.Send);
                } else {
                    Dispatcher.Invoke(new Action(() => {
                        this.poster = (BitmapImage)FindResource("NoPoster");
                    }), DispatcherPriority.Send);
                }
            }
            SetPoster();
        }

        private void SetPoster() {
            Dispatcher.Invoke(new Action(() => {              
                posterImage.Source = poster;
                Storyboard sb = (Storyboard)FindResource("OpacityUp");
                Storyboard temp = sb.Clone();
                temp.FillBehavior = FillBehavior.Stop;
                temp.Completed += (se, ev) => {
                    posterImage.Opacity = 1;
                };
                temp.Begin(posterImage);
            }), DispatcherPriority.Send);
        }

        private void GetInfo(int id) {
            if (!hasBeenUpdated) {
                series = Series.GetSeries(id);
                hasBeenUpdated = true;
            }
            SetInfo();
        }

        private void SetInfo() {
            Dispatcher.Invoke(new Action(() => {
                genres.Text = "";
                for (int i = 0; i < series.genre.Count; i++) {
                    if (i != series.genre.Count - 1) {
                        genres.Text += series.genre[i] + ", ";
                    } else {
                        genres.Text += series.genre[i];
                    }
                }
                showName.Text = series.seriesName;
                
                schedule.Text = series.airsDayOfWeek + " at " + series.airsTime;
                network.Text = series.network;
                stat.Text = series.status;
                prem.Text = series.firstAired;
                len.Text = series.runtime;
                summary.Text = series.overview;
                agerating.Text = series.rating;
                rating.Text = series.siteRating + "/10";
                Storyboard sb = (Storyboard)FindResource("OpacityDown");
                Storyboard temp = sb.Clone();
                temp.FillBehavior = FillBehavior.Stop;
                temp.Completed += (s, e) => {
                    LoadingText.Opacity = 0;
                    Storyboard sbUp = (Storyboard)FindResource("OpacityUp");
                    Storyboard tempUp = sbUp.Clone();
                    tempUp.FillBehavior = FillBehavior.Stop;
                    tempUp.Completed += (sa,ea) => { DataPart.Opacity = 1;  };
                    tempUp.Begin(DataPart);
                };
                temp.Begin(LoadingText);
            }), DispatcherPriority.Send);
        }

        private void showName_MouseUp(object sender, MouseButtonEventArgs e) {
            Process.Start("http://www.imdb.com/title/" + series.imdbId + "/?ref_=fn_al_tt_1");
        }
    }
}
