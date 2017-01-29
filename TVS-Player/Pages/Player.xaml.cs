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
        public Player(string p) {
            InitializeComponent();
            ClickTimer = new Timer(300);
            ClickTimer.Elapsed += new ElapsedEventHandler(EvaluateClicks);
            path = p;
        }
        Timer hideMenu;
        Timer moveProgress;
        bool isplaying = true;
        bool maximized = false;
        private Timer ClickTimer;
        private int ClickCounter;

        private void ShowHideMenu(string Storyboard, StackPanel pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e) {
            Dispatcher.Invoke(new Action(() => {
                ShowHideMenu("sbHideBottomMenu", panelMenu);
                System.Windows.Forms.Cursor.Hide();
            }), DispatcherPriority.Send);
            MediaElement.MouseMove += overlayTrigger_MouseEnter;
            hideMenu.Stop();
        }

        private void overlayTrigger_MouseEnter(object sender, MouseEventArgs e) {
            ShowHideMenu("sbShowBottomMenu", panelMenu);
            MediaElement.MouseMove -= overlayTrigger_MouseEnter;
            System.Windows.Forms.Cursor.Show();
            hideMenu = new Timer();
            hideMenu.Stop();
            hideMenu.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            hideMenu.Interval = 3000;
            hideMenu.Start();
        }


        private void MediaElement_Loaded(object sender, RoutedEventArgs e) {
            MediaElement.Source = new Uri(path);
            MediaElement.LoadedBehavior = MediaState.Manual;
            MediaElement.Play();
            SoundLevel.Value = 0.5;
        }

        private void Play_Click(object sender, RoutedEventArgs e) {
            if (isplaying) {
                isplaying = false;
                MediaElement.Pause();
                moveProgress.Stop();
                PlayPauseText.Text = "Play";
                BitmapImage img = new BitmapImage(new Uri("../Icons/play-button.png", UriKind.Relative));
                PlayPauseImage.Source = img;
            } else {
                isplaying = true;
                MediaElement.Play();
                moveProgress.Start();
                PlayPauseText.Text = "Pause";
                BitmapImage img = new BitmapImage(new Uri("../Icons/pause.png", UriKind.Relative));
                PlayPauseImage.Source = img;
            }
        }

        private void PrevEpButton_Click(object sender, RoutedEventArgs e) {

        }

        private void NextEpButton_Click(object sender, RoutedEventArgs e) {

        }

        private void MuteButton_Click(object sender, RoutedEventArgs e) {
            if (MediaElement.IsMuted) {
                MediaElement.IsMuted = false;
                BitmapImage img = new BitmapImage(new Uri("../Icons/speaker.png", UriKind.Relative));
                MuteImage.Source = img;
            } else {
                MediaElement.IsMuted = true;
                BitmapImage img = new BitmapImage(new Uri("../Icons/mute.png", UriKind.Relative));
                MuteImage.Source = img;

            }

        }
        private void SoundLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            MediaElement.Volume = SoundLevel.Value;
            if (MediaElement.Volume == 0) {
                MediaElement.IsMuted = true;
                BitmapImage img = new BitmapImage(new Uri("../Icons/mute.png", UriKind.Relative));
                MuteImage.Source = img;
            } else {
                BitmapImage img = new BitmapImage(new Uri("../Icons/speaker.png", UriKind.Relative));
                MuteImage.Source = img;
            }
        }
        private void MoveBar(double speed) {
            Dispatcher.Invoke(new Action(() => {
                Progress.Value += speed;
            }), DispatcherPriority.Send);
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e) {
            Progress.Maximum = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            System.Windows.Forms.Cursor.Hide();
            moveProgress = new Timer();
            moveProgress.Interval = 100;
            double countSpeed = MediaElement.NaturalDuration.TimeSpan.TotalMilliseconds/10000;
            moveProgress.Elapsed += (s, ev) => MoveBar(countSpeed);

            moveProgress.Start();
        }
        private void Progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            MediaElement.Position = TimeSpan.FromMilliseconds(Progress.Value);
        }

        private void MediaElement_MouseUp(object sender, MouseButtonEventArgs e) {
            ClickTimer.Stop();
            ClickCounter++;
            ClickTimer.Start();

        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void EvaluateClicks(object source, ElapsedEventArgs e) {
            ClickTimer.Stop();
            if (ClickCounter == 2) {
                if (!maximized) {
                    maximized = true;
                    Dispatcher.Invoke(new Action(() => {
                        Window main = Window.GetWindow(this);
                        main.WindowStyle = WindowStyle.None;
                        main.WindowState = WindowState.Maximized;
                    }), DispatcherPriority.Send);
                } else {
                    maximized = false;
                    Dispatcher.Invoke(new Action(() => {
                        Window main = Window.GetWindow(this);
                        main.WindowStyle = WindowStyle.SingleBorderWindow;
                        main.WindowState = WindowState.Normal;
                    }), DispatcherPriority.Send);
                }
            }
            ClickCounter = 0;
        }
    }
}
