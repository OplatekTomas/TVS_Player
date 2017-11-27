using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public TorrentUserControl(TorrentDownloader torrent,StackPanel panel) {
            InitializeComponent();
            downloader = torrent;
            this.panel = panel;
        }
        TorrentDownloader downloader;
        StackPanel panel;
        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Task.Run(() => {
                bool isLoaded = true;
                Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                while (isLoaded) {
                    Dispatcher.Invoke(() => SetInfo(),DispatcherPriority.Send);
                    Thread.Sleep(1000);
                    Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                }
            });
        }

        private void SetInfo() {
            TorrentName.Text = downloader.Handle.TorrentFile != null ? downloader.Handle.TorrentFile.Name : "Downloading metadata";
            DownloadSpeed.Text = GetSpeed(downloader.Status.DownloadRate);
            UploadSpeed.Text = GetSpeed(downloader.Status.UploadRate);
            SetValue(downloader.Status.Progress);
            Percentage.Text = Math.Round(Progress.Value * 100, 1) + "%";
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

        private void Pause_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Pause_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void Pause_MouseUp(object sender, MouseButtonEventArgs e) {
            if (downloader.IsPaused) {
                downloader.Resume();
                PauseIcon.SetResourceReference(Image.SourceProperty, "PauseIcon");
            } else {
                downloader.Pause();
                PauseIcon.SetResourceReference(Image.SourceProperty, "PlayIcon");
            }
        }

        private async void Remove_MouseUp(object sender, MouseButtonEventArgs e) {
            var result = await MessageBox.Show("Do you also want to delete the files?", "Wargning", MessageBoxButtons.YesNoCancel);
            if (result == MessageBoxResult.Yes) {
                downloader.Remove(true);
            } else if(result == MessageBoxResult.No) {
                downloader.Remove(false);
            }
        }

        private void Question_MouseUp(object sender, MouseButtonEventArgs e) {
            Process.Start(downloader.TorrentSource.URL);
        }
    }
}
