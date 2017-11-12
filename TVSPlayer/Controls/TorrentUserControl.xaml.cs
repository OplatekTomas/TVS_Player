using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for TorrentUserControl.xaml
    /// </summary>
    public partial class TorrentUserControl : UserControl {
        public TorrentUserControl(TorrentDownloader torrent) {
            InitializeComponent();
            downloader = torrent;
        }
        TorrentDownloader downloader;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Task.Run(() => {
                while (true) {
                    Dispatcher.Invoke(() => SetInfo(),DispatcherPriority.Send);
                    Thread.Sleep(1000);
                }
            });
        }

        private void SetInfo() {
            TorrentName.Text = downloader.Handle.TorrentFile != null ? downloader.Handle.TorrentFile.Name : "Downloading metadata";
            DownloadSpeed.Text = GetSpeed(downloader.Status.DownloadRate);
            UploadSpeed.Text = GetSpeed(downloader.Status.UploadRate);
            SetValue(downloader.Status.Progress*100);
            Percentage.Text = Math.Round(Progress.Value,1) + "%";
        }

        public void SetValue(double value) {
            DoubleAnimation animation = new DoubleAnimation(value, new TimeSpan(0, 0, 0, 0, 200));
            animation.AccelerationRatio = .5;
            animation.DecelerationRatio = .5;
            Progress.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        private string GetSpeed(double speed) {
            string speedText = speed + " B/s";
            if (speed > 1000) {
                speedText = (speed / 1000).ToString("N0") + " kB/s";
            }
            if (speed > 1000000) {
                speedText = (speed / 1000000).ToString("N1") + " MB/s";
            }
            if (speed > 1000000000) {
                speedText = (speed / 1000000000).ToString("N1") + " GB/s";
            }
            return speedText;
        }
    }
}
