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
using System.Linq;
using TVS_Player_Base;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : Page {
        public VideoPlayer(Series series,Episode episode) {
            InitializeComponent();
            _episode = episode;
            _series = series;
        }
        Series _series;
        Episode _episode;
        ScannedFile _file;
        DispatcherTimer _uiUpdateTimer = new DispatcherTimer();
        bool _isFullScreen;
        WindowState _lastState;

        private async void Player_Loaded(object sender, RoutedEventArgs e) {
            _episode = await Episode.GetEpisode(_series.Id, _episode.Id);
            var sb = (Storyboard)FindResource("FadeInSearchBar");
            sb.Begin(TopBar);
            var files = await ScannedFile.GetFiles(_episode.Id);
            var temp = files.FirstOrDefault(x => x.FileType == "Video" || x.FileType == 1.ToString());
            if (temp != default) {
                _file = temp;
                //Disable screen saver
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                //Set events and play video
                Player.MediaFailed += async (s, ev) => await MediaFailed();
                Player.MediaEnded += async (s, ev) => await MediaEnded();
                Player.PreviewMouseLeftButtonUp += Player_MouseLeftButtonUp;
                Player.MediaOpened += async (s, ev) => await MediaOpened();
                Player.Source = new Uri(temp.URL);
                Focus();
            }
        }

        private async Task MediaOpened() {
            //Hiding and stopping buffering animations
            Animate.FadeOut(Loading);
            LoadingAnim.Stop();
            Player.Volume = 0;
            //Setup position slider
            PositionSlider.Maximum = Player.MediaDuration;
            if (Player.MediaDuration >= 36000000000) {
                TimeProgressGrid.Width = new GridLength(110);
            } else {
                TimeProgressGrid.Width = new GridLength(85);
            }
            _uiUpdateTimer.Interval = new TimeSpan(0, 0, 1);
            _uiUpdateTimer.Tick += (s, ev) => UpdateUi();
            _uiUpdateTimer.Start();
            //Handle progress in episode
            if (!_episode.Finished && !string.IsNullOrEmpty(_file.TimeStamp)) {
                Player.MediaPosition = Int32.Parse(_file.TimeStamp) * 10000000;
            }
            if (_episode.Finished) {
                await ScannedFile.SetEpisodeFinished(_episode.Id, false);
            }
        }
        private async Task MediaEnded() {
            Dispatcher.Invoke(() => {
                View.RemovePage();
            });
        }
        private async Task MediaFailed() {
            Dispatcher.Invoke(() => {
                View.RemovePage();
            });
        }

        private void UpdateUi() {
            PositionSlider.Value = Player.MediaPosition;
            TimeProgress.Text = GetLengthString(Player.MediaPosition) + "/" + GetLengthString(Player.MediaDuration);
        }

        DispatcherTimer _sliderMoveTimer = new DispatcherTimer();

        private void PositionSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _sliderMoveTimer.Stop();
            _sliderMoveTimer = new DispatcherTimer();
            _sliderMoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _sliderMoveTimer.Tick += (s, ev) => {
                Player.MediaPosition = Convert.ToInt64(PositionSlider.Value);
                TimeProgress.Text = GetLengthString(Player.MediaPosition) + "/" + GetLengthString(Player.MediaDuration);
            };
            _sliderMoveTimer.Start();
            Player.Pause();
            _uiUpdateTimer.Stop();
        }

        private void PositionSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Player.MediaPosition = Convert.ToInt64(PositionSlider.Value);
            _sliderMoveTimer.Stop();
            _uiUpdateTimer.Start();
            Player.Play();
        }

        [DllImport("kernel32.dll")]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        enum EXECUTION_STATE : uint {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        DispatcherTimer timer = new DispatcherTimer();
        bool isRunning = false;
        private void Player_MouseMove(object sender, MouseEventArgs e) {
            if (!isRunning) {
                isRunning = true;
                Mouse.OverrideCursor = null;
                var sb = (Storyboard)FindResource("FadeInSearchBar");
                Animate.FadeIn(BottomPanel);
                BottomPanel.BeginAnimation(Border.MarginProperty, new ThicknessAnimation(new Thickness(10, 0, 10, 0), TimeSpan.FromMilliseconds(200)));
                sb.Begin(TopBar);
                if (timer != null) {
                    timer.Tick -= HideUi;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 2, 500);
                timer.Tick += HideUi;
                timer.Start();
            }

        }
        private void HideUi(object sender, EventArgs e) {
            var sb = (Storyboard)FindResource("FadeOutSearchBar");
            BottomPanel.BeginAnimation(Border.MarginProperty, new ThicknessAnimation(new Thickness(10, 0, 10, -40), TimeSpan.FromMilliseconds(200)));
            Animate.FadeOut(BottomPanel);
            sb.Begin(TopBar);
            Mouse.OverrideCursor = Cursors.None;
            isRunning = false;
        }

        private void Player_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Player.IsPlaying) {
                Player.Pause();
                Animate.FadeOut(PausePlayIcon, () => {
                    PausePlayIcon.Source = (BitmapImage)FindResource("Play");
                    Animate.FadeIn(PausePlayIcon);
                });
            } else {
                Player.Play();
                Animate.FadeOut(PausePlayIcon, () => {
                    PausePlayIcon.Source = (BitmapImage)FindResource("Pause");
                    Animate.FadeIn(PausePlayIcon);
                });
            }
            e.Handled = true;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Player.Volume = VolumeSlider.Value;
            if (VolumeSlider.Value > VolumeSlider.Minimum && e.OldValue == VolumeSlider.Minimum) {
                Animate.FadeOut(VolumeIcon, () => {
                    VolumeIcon.Source = (BitmapImage)FindResource("VolumeNormal");
                    Animate.FadeIn(VolumeIcon);
                });
            } else if(VolumeSlider.Value == VolumeSlider.Minimum) {
                Animate.FadeOut(VolumeIcon, () => {
                    VolumeIcon.Source = (BitmapImage)FindResource("VolumeMuted");
                    Animate.FadeIn(VolumeIcon);
                });
                Player.Volume = 0;
            }
        }

        double _tempVolume;
        private void MuteVolume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Player.Volume > 0) {
                _tempVolume = Player.Volume;
                VolumeSlider.Value = VolumeSlider.Minimum;
            } else {
                VolumeSlider.Value = _tempVolume;

            }
        }

        private void Settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void FullScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HandleFullScreen();
        }

        private string GetLengthString(long value) {
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

        private void Player_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {          
            if (e.ClickCount == 2) {
                HandleFullScreen();
            }
        }

        private void HandleFullScreen() {
            var window = (MainWindow)Application.Current.MainWindow;
            if (!_isFullScreen) {
                _lastState = window.WindowState;
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowState = WindowState.Maximized;
            } else {
                window.WindowState = _lastState;
                window.ResizeMode = ResizeMode.CanResize;
                window.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            _isFullScreen = !_isFullScreen;
        }

        private async void Player_Unloaded(object sender, RoutedEventArgs e) {
            //Re-enable screen saver
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            //Make sure mouse is visible and won't hide
            PreviewMouseMove -= Player_MouseMove;
            timer.Stop();
            Mouse.OverrideCursor = null;
            if (_isFullScreen) {
                HandleFullScreen();
            }
            //Send progress of what has been viewed back to server (minus couple of seconds) or if its near end just send info that viewing has finished
            if (Player.MediaDuration - 3000000000 < Player.MediaPosition) {
                await ScannedFile.SetEpisodeFinished(_episode.Id, true);
            } else {
                await ScannedFile.SetEpisodeProgress(_episode.Id, _file.Id, Player.MediaPosition / 10000000);
            }
        }

    }

}
