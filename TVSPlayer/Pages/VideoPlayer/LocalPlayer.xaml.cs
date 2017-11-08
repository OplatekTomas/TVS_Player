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
        public Series series;
        public Episode episode;
        ScannedFile scannedFile;
        string fileLenght;
        DispatcherTimer positionUpdate = new DispatcherTimer();

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            MainWindow.videoPlayback = true;
            PlayerPage.Focus();
            EpisodeName.Text = Helper.GenerateName(series, episode);
            VolumeSlider.Value = Player.Volume = Properties.Settings.Default.Volume;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            Player.MediaOpened += (s, ev) => MediaOpenedEvent();
            Player.MediaEnded += (s, ev) => MediaFinishedEvent();
            Player.Source = new Uri(scannedFile.NewName);
        }

        private void MediaFinishedEvent() {
            Return();
        }

        private void MediaOpenedEvent() {
            VideoSlider.Maximum = Player.MediaDuration;
            if (!episode.finised) { Player.MediaPosition = episode.continueAt; }
            episode.finised = false;
            fileLenght = GetTime(Player.MediaDuration);
            positionUpdate.Interval = new TimeSpan(0, 0, 1);
            positionUpdate.Tick += new EventHandler(UpdatePosition);
            positionUpdate.Start();
            Player.Play();
        }

        private string GetTime(long value) {
            int minutes, seconds, hours;
            minutes = seconds = hours = 0;
            value = value / 10000000;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value - 60 * hours) / 60));
            seconds = Convert.ToInt32(Math.Floor((double)(value - (60 * hours) - (60 * minutes))));
            string hoursString = hours > 0 ? hours + "0" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
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
            if (isPlaying) { 
                Player.Play();
            }
            positionUpdate.Start();
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

        private void PlayIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Pause();
        }

        private void Page_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
            }
        }

        private void VideoSlider_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                GoForward();
            } else {
                GoBack();
            }
        }

        private void BackIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            GoBack();
        }

        private void ForwardIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            GoForward();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Properties.Settings.Default.Volume = Player.Volume = VolumeSlider.Value;
            Task.Run(() => {
                Properties.Settings.Default.Save();
            });
            if (VolumeSlider.Value == VolumeSlider.Minimum) {
                Player.Volume = 0;
            }
        }

        private void FullscreenIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
        }

        private void AlwaysTopIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SwitchState(MainWindow.PlayerState.PiP);
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Return();
        }

        //Yes I do know this can be simplified but that would result in slower pause and I do not want that
        bool isPlaying = true;
        private void Pause() {
            if (isPlaying) {
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
            isPlaying = !isPlaying;
        }

        double lastValue = 0.85;
        private void Mute() {
            if (Player.Volume == 0) {
                Player.Volume = lastValue;
            } else {
                lastValue = Player.Volume;
                Player.Volume = 0;
            }
        }

        private void GoForward() {
            Player.MediaPosition += 100000000;
        }

        private void GoBack() {
            Player.MediaPosition -= 100000000;
        }

        private void Return() {
            timer.Stop();
            MainWindow.videoPlayback = false;
            MainWindow.SwitchState(MainWindow.PlayerState.Normal);
            Mouse.OverrideCursor = null;
            episode.continueAt = Player.MediaPosition - 50000000 > 0 ? Player.MediaPosition - 50000000 : 0;
            episode.finised = Player.MediaDuration - 3000000000 < Player.MediaPosition ? true : false;
            Database.EditEpisode(series.id, episode.id, episode);
            MainWindow.RemovePage();
            MainWindow.SetPage(new SeriesEpisodes(series));
        }

        private void PlayerPage_PreviewKeyUp(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Space:
                    Pause();
                    break;
                case Key.Escape:
                    Return();
                    break;
                case Key.F:
                    MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
                    break;
                case Key.M:
                    Mute();
                    break;
                case Key.Right:
                    GoForward();
                    break;
                case Key.Left:
                    GoBack();
                    break;
            }
        }

        private void PlayerPage_LostFocus(object sender, RoutedEventArgs e) {

        }
    }
}
