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
            using (var session = new Session()) {
                session.ListenOn(6881, 6889);
                var addParams = new AddTorrentParams();
                addParams.SavePath = "C:\\";
                addParams.Url = torrent.magnet;
                handle = session.AddTorrent(addParams);
                handle.SequentialDownload = sequential;
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => {
                    MainWindow.torrents.Add(handle);
                }), DispatcherPriority.Send);
                
            }
        }
    }
}
