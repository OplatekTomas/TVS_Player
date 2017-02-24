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
using System.Timers;
using System.Windows.Threading;
using Cursor = System.Windows.Forms;
using System.Windows.Forms;
using Timer = System.Timers.Timer;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Page {
        string path;
        Episode episode;
        Show selectedShow;
        public Player(string p, Episode e, Show ss) {
            InitializeComponent();
            path = p;
            episode = e;
            selectedShow = ss;
            progressMove = new Timer();
        }
        List<SubtitleItem> subs;
        Timer menuTimer;
        Timer progressMove;
        bool renderSubs = true;
        int subIndex = 0;
        string length;
        private void MediaEl_Loaded(object sender, RoutedEventArgs e) {
            MediaEl.Source = new Uri(path);
            MediaEl.Play();
            MediaEl.ScrubbingEnabled = true;   
        }

        private void MediaEl_MediaOpened(object sender, RoutedEventArgs e) {
            LoadSubs();
            LoadInfo();
            MoveProgress();
            Progress.Maximum = MediaEl.NaturalDuration.TimeSpan.TotalSeconds;
            Window main = System.Windows.Application.Current.MainWindow;
            main.KeyDown += KeyboardShortucs;
            SoundLevel.Value = MediaEl.Volume;
        }

        private void MoveProgress() {
            progressMove.Elapsed += (s, e) => RefreshProgress();
            progressMove.Interval = 1000;
            progressMove.Start();
        }

        private void RefreshProgress() {
            string text;
            Dispatcher.Invoke(new Action(() => {
                if (MediaEl.NaturalDuration.TimeSpan.Hours > 0) {
                    text = MediaEl.Position.ToString(@"h\:mm\:ss") + "/" + length;
                } else {
                    text = MediaEl.Position.ToString(@"mm\:ss") + "/" + length;
                }
                Progress.Value = MediaEl.Position.TotalSeconds;
                ClockText.Text = text;
            }), DispatcherPriority.Send);
        }

        private void LoadInfo() {
            int height = MediaEl.NaturalVideoHeight;
            if (MediaEl.NaturalDuration.TimeSpan.Hours > 0) {
                length = MediaEl.NaturalDuration.TimeSpan.ToString(@"h\:mm\:ss");
                ClockText.Text = "0:00:00/" + length;
            } else {
                length = MediaEl.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                ClockText.Text = "00:00/" + length;
            }
            int width = MediaEl.NaturalVideoWidth;
            var ffProbe = new NReco.VideoInfo.FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(path);
            var res = videoInfo.Streams;
            string codec = res[0].CodecName.ToString();
            Info.Text = width + "x" + height + " " + codec;
            Name.Text = selectedShow.name + " - " + Episodes.getEPOrder(episode) + " - " + episode.name;
        }

        private void LoadSubs() {
            foreach (string file in episode.locations) {
                if (System.IO.Path.GetExtension(file) == ".srt") {
                    subs = SrtParser.ParseStream(file, Encoding.Default);
                    Subs();
                    break;
                }
            }
        }
   
        private void Subs() {
            Action rs = () => RenderSubs();
            Thread t = new Thread(rs.Invoke);
            t.IsBackground = true;           
            t.Start();
        }

        private void RenderSubs() {
            while (renderSubs) {
                SubtitleItem sub = subs[subIndex];
                Dispatcher.Invoke(new Action(() => {
                    double position = Math.Ceiling(MediaEl.Position.TotalMilliseconds);
                    for (int option = 0; option < 6; option++) {
                        if (sub.StartTimes[option] == position) {
                            Subtitles.Text = sub.line;
                        }
                        if (sub.EndTimes[option] == position) {
                            Subtitles.Text = "";
                            subIndex++;
                        }
                    }
                }), DispatcherPriority.Send);
                Thread.Sleep(2);
            }
        }
        private void ShowHideMenu(string Storyboard, Grid pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);

        }
        private void MediaEl_MouseMove(object sender, MouseEventArgs e) {
            ShowHideMenu("StoryBoardShow", TopPanel);
            ShowHideMenu("StoryBoardShow", BottomPanel);
            if (menuTimer != null) {
                menuTimer.Stop();
            }
            Base.MouseMove -= MediaEl_MouseMove;
            menuTimer = new Timer();
            menuTimer.Elapsed += (s,ea) => HideMenu();
            menuTimer.Interval = 1500;
            menuTimer.Start();
        }
        private void HideMenu() {
            Base.MouseMove += MediaEl_MouseMove;
            if (menuTimer != null) {
                menuTimer.Stop();
            }
            Dispatcher.Invoke(new Action(() => {
                ShowHideMenu("StoryBoardHide", TopPanel);
                ShowHideMenu("StoryBoardHide", BottomPanel);
            }), DispatcherPriority.Send);
        }
        private double VolumeLevel;
        private void Mute() {
            if (MediaEl.IsMuted) {
                SoundLevel.Value = VolumeLevel;
                MediaEl.IsMuted = false;
                BitmapImage img = new BitmapImage(new Uri("../Icons/speaker.png", UriKind.Relative));
                MuteImage.Source = img;
            } else {
                VolumeLevel = MediaEl.Volume;
                SoundLevel.Value = 0;
                MediaEl.IsMuted = true;
                BitmapImage img = new BitmapImage(new Uri("../Icons/mute.png", UriKind.Relative));
                MuteImage.Source = img;

            }
        }
        private bool maximized = false;
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
        private bool isplaying = true;
        private void PlayPause() {
            if (isplaying) {
                isplaying = false;
                MediaEl.Pause();
                BitmapImage img = new BitmapImage(new Uri("../Icons/play-button.png", UriKind.Relative));
                PlayPauseButton.Source = img;
            } else {
                isplaying = true;
                MediaEl.Play();
                BitmapImage img = new BitmapImage(new Uri("../Icons/pause.png", UriKind.Relative));
                PlayPauseButton.Source = img;
            }
        }
        private void Quit() {
            menuTimer.Stop();
            progressMove.Stop();
            MediaEl.Close();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
            ((MainWindow)main).KeyUp -= KeyboardShortucs;
            System.Windows.Forms.Cursor.Show();
            if (maximized) {
                main.WindowStyle = WindowStyle.SingleBorderWindow;
                main.WindowState = WindowState.Normal;
            }
        }
        private void GetIndex() {
            try {
                SubtitleItem s = subs.Aggregate((x, y) => Math.Abs(x.StartTime - MediaEl.Position.TotalMilliseconds) < Math.Abs(y.StartTime - MediaEl.Position.TotalMilliseconds) ? x : y);
                subIndex = subs.IndexOf(s) + 1;
            } catch (Exception) { }
         }

        private void PlayPauseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            PlayPause();
        }

        private void MuteImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Mute();
        }

        private void FullScreenImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            FullScreen();
        }

        private void Progress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            Progress.ValueChanged -= Progress_ValueChanged;
        }

        private void Progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            MediaEl.Position = TimeSpan.FromSeconds(Progress.Value);
            Progress.ValueChanged += Progress_ValueChanged;
            Subtitles.Text = "";
            GetIndex();
        }

        private void Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            MediaEl.Position = TimeSpan.FromSeconds(Progress.Value);
        }

        private void Progress_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            MediaEl.Position = TimeSpan.FromSeconds(Progress.Value);
            Progress.ValueChanged += Progress_ValueChanged;
            Subtitles.Text = "";
            GetIndex();
        }

        private void SoundLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            MediaEl.Volume = SoundLevel.Value;
            if (MediaEl.Volume == 0) {
                MediaEl.IsMuted = true;
                BitmapImage img = new BitmapImage(new Uri("../Icons/mute.png", UriKind.Relative));
                MuteImage.Source = img;
            } else {
                MediaEl.IsMuted = false;
                BitmapImage img = new BitmapImage(new Uri("../Icons/speaker.png", UriKind.Relative));
                MuteImage.Source = img;
            }
        }

        private void BackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Quit();
        }

        private void KeyboardShortucs(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.M:
                    Mute();
                    break;
                case Key.F:
                    FullScreen();
                    break;
                case Key.Escape:
                    Quit();
                    break;
                case Key.Right:
                    MediaEl.Position = MediaEl.Position.Add(new TimeSpan(0, 0, 10));
                    Subtitles.Text = "";
                    GetIndex();
                    break;
                case Key.Left:
                    MediaEl.Position = MediaEl.Position.Add(new TimeSpan(0, 0, -10));
                    Subtitles.Text = "";
                    GetIndex();
                    break;
                case Key.Space:
                    PlayPause();
                    break;
                case Key.Up:
                    SoundLevel.Value += 0.1;
                    break;
                case Key.Down:
                    SoundLevel.Value -= 0.1;
                    break;
                case Key.MediaPlayPause:
                    PlayPause();
                    break;

            }
        }
        private Timer ClickTimer;
        private int ClickCounter;
        private void MediaEl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (ClickTimer != null) { 
                ClickTimer.Stop();
            }
            ClickTimer = new Timer(300);
            ClickTimer.Elapsed += new ElapsedEventHandler(EvaluateClicks);
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
    }
}
