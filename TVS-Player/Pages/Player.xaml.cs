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
using Microsoft.DirectX.AudioVideoPlayback;
using Microsoft.DirectX;
using System.Timers;
using System.Windows.Threading;
using Cursor = System.Windows.Forms;
using System.Windows.Forms;
using Timer = System.Timers.Timer;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Page {
        string path;
        DispatcherTimer hideMenu;
        Episode episode;
        Show selectedShow;

        public Player(string p, Episode e,Show ss) {
            InitializeComponent();
            ClickTimer = new Timer(300);
            ClickTimer.Elapsed += new ElapsedEventHandler(EvaluateClicks);
            path = p;
            hideMenu = new DispatcherTimer();
            hideMenu.Interval = TimeSpan.FromSeconds(1);
            hideMenu.Tick += OnTimedEvent;
            episode = e;
            selectedShow = ss;
        }
        Timer moveProgress;
        bool isplaying = true;
        bool maximized = false;
        private Timer ClickTimer;
        private int ClickCounter;
        private double VolumeLevel;

        private void ShowHideMenu(string Storyboard, StackPanel pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);
        }
        private void ShowHideMenu(string Storyboard, Grid pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);
        }
        private void OnTimedEvent(object source, EventArgs e) {
            BackgroundGrid.MouseMove += overlayTrigger_MouseEnter;
            Dispatcher.Invoke(new Action(() => {
                ShowHideMenu("sbHideBottomMenu", panelMenu);
                ShowHideMenu("sbHideTopMenu", topPanel);
                System.Windows.Forms.Cursor.Hide();
            }), DispatcherPriority.Send);
            hideMenu.Stop();
        }

        private void overlayTrigger_MouseEnter(object sender, MouseEventArgs e) {
            ShowHideMenu("sbShowBottomMenu", panelMenu);
            ShowHideMenu("sbShowTopMenu", topPanel);
            System.Windows.Forms.Cursor.Show();
            BackgroundGrid.MouseMove -= overlayTrigger_MouseEnter;
            hideMenu.Stop();
            hideMenu.Start();
        }
            private void MediaElement_Loaded(object sender, RoutedEventArgs e) {
            MediaElement.Source = new Uri(path);
            MediaElement.LoadedBehavior = MediaState.Manual;
            MediaElement.Play();
            SoundLevel.Value = 0.5;
        }

        private void SoundLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            MediaElement.Volume = SoundLevel.Value;
            if (MediaElement.Volume == 0) {
                MediaElement.IsMuted = true;
                BitmapImage img = new BitmapImage(new Uri("../Icons/mute.png", UriKind.Relative));
                MuteImage.Source = img;
            } else {
                MediaElement.IsMuted = false;
                BitmapImage img = new BitmapImage(new Uri("../Icons/speaker.png", UriKind.Relative));
                MuteImage.Source = img;
            }
        }
        private void MoveBar() {
            Dispatcher.Invoke(new Action(() => {
                Progress.Value = MediaElement.Position.TotalSeconds;
            }), DispatcherPriority.Send);
        }

        private void Clock(TimeSpan videoLenght) {
            Dispatcher.Invoke(new Action(() => {
                if (videoLenght.Hours == 0) {
                    ClockText.Text = MediaElement.Position.ToString(@"mm\:ss") +"/"+ videoLenght.ToString(@"mm\:ss");
                } else {
                    ClockText.Text = MediaElement.Position.ToString(@"h\:mm\:ss")+"/" + videoLenght.ToString(@"h\:mm\:ss");
                }
            }), DispatcherPriority.Send);
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e) {
            TimeSpan VideoLenght = MediaElement.NaturalDuration.TimeSpan;
            Timer clock = new Timer();
            clock.Interval = 1000;
            clock.Elapsed += (s, ea) => Clock(VideoLenght);
            Progress.Maximum = VideoLenght.TotalSeconds;
            System.Windows.Forms.Cursor.Hide();
            moveProgress = new Timer();
            moveProgress.Interval = 1000;
            moveProgress.Elapsed += (s, eb) => MoveBar();
            clock.Start();
            moveProgress.Start();
            EPName.Text = episode.name;
            ShowName.Text = selectedShow.name;
            SeasonInfo.Text = getEPOrder(episode);
            FileInfo.Text = getFileInfo();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).KeyUp += BackgroundGrid_KeyUp;

        }

        private string getFileInfo() {
            int height = MediaElement.NaturalVideoHeight;
            int width = MediaElement.NaturalVideoWidth;
            var ffProbe = new NReco.VideoInfo.FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(path);
            var res = videoInfo.Streams;
            string codec = res[0].CodecName.ToString();
            return width + "x" + height + " " + codec;
        }

        private string getEPOrder(Episode e) {
            int season = e.season;
            int episode = e.episode;
            if (season < 10) {
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

        private void MediaElement_MouseUp(object sender, MouseButtonEventArgs e) {
            ClickTimer.Stop();
            ClickCounter++;
            ClickTimer.Start();

        }
        private void EvaluateClicks(object source, ElapsedEventArgs e) {
            ClickTimer.Stop();
            if (ClickCounter == 2) {
                Dispatcher.Invoke(new Action(() => {
                    FullScreen();
                }), DispatcherPriority.Send);
            }
            if (ClickCounter == 1) {
                Dispatcher.Invoke(new Action(() => {
                    PlayPause();
                }), DispatcherPriority.Send);
            }
            ClickCounter = 0;
        }

        private void FullScreen() {
            if (!maximized) {
                maximized = true;
                Window main = Window.GetWindow(this);
                main.WindowStyle = WindowStyle.None;
                main.WindowState = WindowState.Maximized;
            } else {
                maximized = false;
                Window main = Window.GetWindow(this);
                main.WindowStyle = WindowStyle.SingleBorderWindow;
                main.WindowState = WindowState.Normal;
            }
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Quit();   
        }

        private void BackgroundGrid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.F:
                    FullScreen();
                    break;
                case Key.Space:
                    PlayPause();
                    break;
                case Key.Right:
                    MediaElement.Position += new TimeSpan(0, 0, 10);
                    break;
                case Key.Left:
                    MediaElement.Position -= new TimeSpan(0, 0, 10);
                    break;
                case Key.M:
                    Mute();
                    break;
                case Key.Escape:
                    Quit();
                    break;
                default:
                    break;
            }
        }
        
        private void Quit() {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
            ((MainWindow)main).KeyUp -= BackgroundGrid_KeyUp;
            System.Windows.Forms.Cursor.Show();
            if (maximized) {
                main.WindowStyle = WindowStyle.SingleBorderWindow;
                main.WindowState = WindowState.Normal;
            }
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            MediaElement.Position = TimeSpan.FromSeconds(Progress.Value);
        }

        private void Progress_DragStarted(object sender, RoutedEventArgs e) {
            Progress.ValueChanged -= Progress_ValueChanged;
        }
        private void Progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            MediaElement.Position = TimeSpan.FromSeconds(Progress.Value);
            Progress.ValueChanged += Progress_ValueChanged;
        }

        private void PlayPause() {
            if (isplaying) {
                isplaying = false;
                MediaElement.Pause();
                moveProgress.Stop();
                BitmapImage img = new BitmapImage(new Uri("../Icons/play-button.png", UriKind.Relative));
                PlayPauseButton.Source = img;
            } else {
                isplaying = true;
                MediaElement.Play();
                moveProgress.Start();
                BitmapImage img = new BitmapImage(new Uri("../Icons/pause.png", UriKind.Relative));
                PlayPauseButton.Source = img;
            }
        }

        private void Mute() {
            if (MediaElement.IsMuted) {
                SoundLevel.Value = VolumeLevel;
                MediaElement.IsMuted = false;
                BitmapImage img = new BitmapImage(new Uri("../Icons/speaker.png", UriKind.Relative));
                MuteImage.Source = img;
            } else {
                VolumeLevel = MediaElement.Volume;
                SoundLevel.Value = 0;
                MediaElement.IsMuted = true;
                BitmapImage img = new BitmapImage(new Uri("../Icons/mute.png", UriKind.Relative));
                MuteImage.Source = img;

            }

        }

        private void PlayPauseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            PlayPause();
        }

        private void FullScreenImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            FullScreen();
        }

        private void MuteImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Mute();
        }
    }
}
