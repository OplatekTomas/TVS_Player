using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Shell;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for TorrentStreamer.xaml
    /// </summary>
    public partial class TorrentStreamer : Page {
        public TorrentStreamer(TorrentDownloader downloader) {
            InitializeComponent();
            this.downloader = downloader;
        }
        TorrentDownloader downloader;
        string fileLenght;
        string file;

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            MainWindow.HideContent();
            MainWindow.videoPlayback = true;
            VolumeSlider.Value = Player.Volume = Properties.Settings.Default.Volume;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            Focus();
            Player.MediaOpened += (s, ev) => MediaOpenedEvent();
            while (Player.MediaDuration == 0) {
                Animate();
                await Task.Run(() => {
                    Thread.Sleep(1080);
                });
                file = GetSource();
                if (file != null && File.Exists(file)) {
                    Player.Source = new Uri(file);
                    Player.Stop();
                }
            }
            Player.MediaFailed += (s, ev) => MediaFailedEvent();
            Player.MediaEnded += (s, ev) => MediaFinishedEvent();
            var sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => {
                Middle.Visibility = Visibility.Collapsed;
            };
            sb.Begin(Middle);

        }

        private void Animate() {
            Storyboard sb = (Storyboard)FindResource("Rotate");
            Storyboard temp = sb.Clone();
            temp.Completed += (s, e) => {
                MiddleIcon.RenderTransform = new RotateTransform(1);
            };
            temp.Begin(MiddleIcon);
        }

        DispatcherTimer positionUpdate = new DispatcherTimer();
        private void MediaOpenedEvent() {
            Player.Play();
            if (positionUpdate.IsEnabled) {
                positionUpdate.Stop();
                positionUpdate = new DispatcherTimer();
            }
            positionUpdate.Interval = new TimeSpan(0, 0, 1);
            positionUpdate.Tick += new EventHandler(UpdatePosition);
            positionUpdate.Start();
        }

        private async void UpdatePosition(object sender, EventArgs e) { 
           
        }
   
        private void MediaFailedEvent() {

        }

        private string GetSource() {
            string path = downloader.Status.SavePath + "\\" + downloader.Status.Name;
            if (File.Exists(path)) {
                return path;
            } else if (Directory.Exists(path)) {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                string[] fileExtension = new string[7] { ".mkv", ".m4v", ".avi", ".mp4", ".mov", ".wmv", ".flv" };
                foreach (var file in files) {
                    if (Renamer.IsMatchToIdentifiers(file) && fileExtension.Contains(Path.GetExtension(file).ToLower())) {
                        return file;
                    }
                }
            }
            return null;
        }

        private async void MediaFinishedEvent() {
            // Return();
            if (Player.MediaDuration < Player.MediaPosition) {

            } else {

            }
        }

        private string GetTime(float value) {
            int minutes, seconds, hours;
            minutes = seconds = hours = 0;
            value = value / 10000000;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value / 60 - 60 * hours)));
            seconds = Convert.ToInt32(Math.Floor((double)(value - ((60 * 60 * hours) + (60 * minutes)))));
            string hoursString = hours > 0 ? hours + ":" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
        }

        DispatcherTimer contentUpdate = new DispatcherTimer();

        private void VideoSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            contentUpdate.Stop();
            contentUpdate = new DispatcherTimer();
            contentUpdate.Interval = new TimeSpan(0, 0, 0, 0, 500);
            contentUpdate.Tick += (s, ev) => {
                CurrentTime.Text = GetTime(Player.MediaPosition) + "/" + fileLenght;
            };
            contentUpdate.Start();
            Player.Pause();
            positionUpdate.Stop();
        }

        private void VideoSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
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

        private async void Return() {
            MainWindow.ShowContent();
            MouseMove -= Page_MouseMove;
            timer.Stop();
            MainWindow.videoPlayback = false;
            MainWindow.SwitchState(MainWindow.PlayerState.Normal);
            MainWindow.RemovePage();
            MainWindow.SetPage(new SeriesEpisodes(downloader.TorrentSource.Series));
            Mouse.OverrideCursor = null;
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
