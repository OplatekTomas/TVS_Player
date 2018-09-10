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
    public partial class VideoPlayer : Page
    {
        public VideoPlayer(Episode episode){
            InitializeComponent();
            _episode = episode;
        }
        Episode _episode;

        private void DisableScreenSaver() {

        }

        private async void Player_Loaded(object sender, RoutedEventArgs e) {
            var sb = (Storyboard)FindResource("FadeInSearchBar");
            sb.Begin(TopBar);
            var files = await ScannedFile.GetFiles(_episode.Id);
            var temp = files.FirstOrDefault(x => x.FileType == "Video" || x.FileType == 1.ToString());
            if (temp != default) {
                //Disable screen saver
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
                //Set events and play video
                Player.MediaFailed += async (s, ev) => await MediaFailed();
                Player.MediaEnded += async (s, ev) => await MediaEnded();
                Player.MediaOpened += async (s, ev) => await MediaOpened();
                Player.Source = new Uri(temp.URL);
                Focus();
            }
        }

        private async Task MediaOpened() {
            Animate.FadeOut(Loading);
        }
        private async Task MediaEnded() {

        }
        private async Task MediaFailed() {

        }

        private void Player_Unloaded(object sender, RoutedEventArgs e) {
            //Re-enable screen saver
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            //Make sure mouse is visible and won't hide
            PreviewMouseMove -= Player_MouseMove;
            timer.Stop();
            Mouse.OverrideCursor = null;
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
                sb.Begin(TopBar);
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
            var sb = (Storyboard)FindResource("FadeOutSearchBar");
            sb.Begin(TopBar);
            Mouse.OverrideCursor = Cursors.None;
            isRunning = false;
        }

        private void BackBtn_MouseEnter(object sender, MouseEventArgs e) {

        }
    }

}
