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
using System.Threading;
using System.Diagnostics;

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
        }
        List<SubtitleItem> subs;
        bool renderSubs = true;
        int subIndex = 0;
        private void MediaEl_Loaded(object sender, RoutedEventArgs e) {
            MediaEl.Source = new Uri(path);
            //MediaEl.Play();
            MediaEl.ScrubbingEnabled = true;            
        }

        private void MediaEl_MediaOpened(object sender, RoutedEventArgs e) {
            subs = SrtParser.ParseStream(Encoding.Default);
            Subs();
        }
        private void Subs() {
            Action rs = () => RenderSubs();
            Thread t = new Thread(rs.Invoke);
            t.IsBackground = true;           
            t.Start();
        }

        private void timetest() {
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
    }
}
