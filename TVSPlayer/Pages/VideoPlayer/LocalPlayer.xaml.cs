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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;
using static TVS.API.Episode;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for LocalPlayer.xaml
    /// </summary>
    public partial class LocalPlayer : Page
    {
        public LocalPlayer(Series series, Episode episode, ScannedFile scannedFile)
        {
            InitializeComponent();
            this.series = series;
            this.episode = episode;
            this.scannedFile = scannedFile;
        }
        Series series;
        Episode episode;
        ScannedFile scannedFile;
        string fileLenght;
        DispatcherTimer positionUpdate = new DispatcherTimer();

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            MainWindow.videoPlayback = true;
            Player.MediaOpened += (s, ev) => MediaOpenedEvent();
            Player.Source = new Uri(scannedFile.NewName);
        }

        private string GetTime(long value) {
            int minutes, seconds, hours;
            minutes = seconds = hours = 0;
            value = value / 10000000;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value - 60 * hours) / 60));
            seconds = Convert.ToInt32(Math.Floor((double)(value-(60*hours)-(60*minutes))));
            string hoursString = hours > 0 ? hours + "0" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
        }

        private void MediaOpenedEvent() {
            VideoSlider.Maximum = Player.MediaDuration;
            fileLenght = GetTime(Player.MediaDuration);
            positionUpdate.Interval = new TimeSpan(0, 0, 1);
            positionUpdate.Tick += new EventHandler(UpdatePosition);
            positionUpdate.Start();
            Player.Play();
        }

        private void UpdatePosition(object sender, EventArgs e) {
            VideoSlider.Value = Player.MediaPosition;
            CurrentTime.Text = GetTime(Player.MediaPosition) + "/" + fileLenght;
        } 

        DispatcherTimer contentUpdate = new DispatcherTimer();
        private void VideoSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            contentUpdate.Stop();
            contentUpdate = new DispatcherTimer();
            contentUpdate.Interval = new TimeSpan(0, 0, 0, 0, 500);
            contentUpdate.Tick += (s, ev) => {
                Player.MediaPosition = Convert.ToInt64(VideoSlider.Value);
                CurrentTime.Text = GetTime(Player.MediaPosition) + "/" + fileLenght;
            };
            contentUpdate.Start();
            Player.Pause();
            positionUpdate.Stop();
        }

        private void VideoSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            Player.MediaPosition = Convert.ToInt64(VideoSlider.Value);
            contentUpdate.Stop();
            Player.Play();
            positionUpdate.Start();
        }

        #region Animations
        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.SetPage(new SeriesEpisodes(series));
        }
        DispatcherTimer timer;
        bool isRunning = false;
        private void Page_MouseMove(object sender, MouseEventArgs e) {
            if (!isRunning) {
                isRunning = true;
                var sb = (Storyboard)FindResource("OpacityUp");
                var clone = sb.Clone();
                Mouse.OverrideCursor = null;
                clone.Completed += (s, ev) => {
                    isRunning = false;

                };
                clone.Begin(TopBar);
                clone.Begin(BottomBar);
                if (timer != null) {
                    timer.Tick -= Timer_Tick;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 1, 500);
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e) {
            var sb = (Storyboard)FindResource("OpacityDown");
            Mouse.OverrideCursor = Cursors.None;
            sb.Begin(TopBar);
            sb.Begin(BottomBar);
        }

        //Yes I do know this can be simplified but that would result in slower pause and I do not want that
        private void PlayIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Player.IsPlaying) {
                Player.Pause();
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) => {
                    ActualPlayIcon.Source = new BitmapImage(new Uri("/Icons/ico-play-light.png", UriKind.Relative));
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(PlayIcon);
                };
                clone.Begin(PlayIcon);
            } else {
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) => {
                    ActualPlayIcon.Source = new BitmapImage(new Uri("/Icons/ico-pause-light.png", UriKind.Relative));
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(PlayIcon);
                };
                clone.Begin(PlayIcon);
                Player.Play();
            }
        }

        #endregion

        private void Page_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                MainWindow.GoFullScreen();
            }
        }

        private void VideoSlider_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                Player.MediaPosition += 100000000;
            } else {
                Player.MediaPosition -= 100000000;
            }
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e) {
            timer.Stop();
            MainWindow.videoPlayback = false;
            MainWindow.GoFullScreen(true);
            Mouse.OverrideCursor = null;
        }

        private void BackIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Player.MediaPosition -= 100000000;
        }

        private void ForwardIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Player.MediaPosition += 100000000;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Player.Volume = VolumeSlider.Value;
            if (VolumeSlider.Value == VolumeSlider.Minimum) {
                Player.Volume = 0;
            }
        }


        private void FullscreenIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.GoFullScreen();
        }
    }
}
