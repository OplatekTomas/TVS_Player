using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ragnar;
using System.Windows.Threading;
using System.Threading;

namespace TVS_Player {
    class TorrentDownloader {
        public TorrentDownloader(TorrentItem torrent) {
            this.torrent = torrent;
        }
        Thread downloadThread;
        TorrentItem torrent;
        TorrentHandle handle;
        public void DownloadTorrent() {
            Notification n = new Notification();
            Action a = () => Down(false);
            downloadThread = new Thread(a.Invoke);
            downloadThread.IsBackground = true;
            downloadThread.Name = "Torrent download";
            downloadThread.Start();
        }
        public void DownloadTorrent(bool sequential) {
            Notification n = new Notification();
            MainWindow.notifications.Add(n);
            Action a = () => Down(sequential);
            downloadThread = new Thread(a.Invoke);
            downloadThread.IsBackground = true;
            downloadThread.Name = "Torrent download";
            downloadThread.Start();
        }
        private void Down(bool sequential) {
            Notification notification = null;
            using (var session = new Session()) {
                session.ListenOn(6881, 6889);
                var addParams = new AddTorrentParams();
                addParams.SavePath = "D:\\";
                addParams.Url = torrent.magnet;
                handle = session.AddTorrent(addParams);
                handle.SequentialDownload = sequential;
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => {
                    notification = MainWindow.AddNotification("","");
                }), DispatcherPriority.Send);
                while (true) {
                    var status = handle.QueryStatus();
                    if (status.IsSeeding) {
                        break;
                    }
                    int speed = status.DownloadRate;
                    string speedText = GetSpeed(speed);
                    string name = torrent.name;
                    notification.MainText.Text = name;
                    notification.SecondText.Text = speedText;
                    notification.ProgBar.Value = status.Progress * 100;
                    Dispatcher.CurrentDispatcher.Invoke(new Action(() => {
                        int index = MainWindow.notifications.IndexOf(notification);
                        MainWindow.notifications[index] = notification;
                    }), DispatcherPriority.Send);
                    Thread.Sleep(100000);
                }
            }
        }
        private string GetSpeed(int speed) {
            string speedText = speed + " B/s";
            if (speed > 1000) {
                speedText = speed / 1000 + " kB/s";
            }
            if (speed > 1000000) {
                speedText = speed / 1000000 + " MB/s";
            }
            if (speed > 1000000000) {
                speedText = speed / 1000000000 + " GB/s";
            }
            return speedText;
        }
    }
}
