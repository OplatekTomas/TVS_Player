using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ragnar;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace TVS_Player {
    class TorrentDownloader {
        public TorrentDownloader(TorrentItem torrent, Episode episode,Show show) {
            this.torrent = torrent;
            this.episode = episode;
            this.show = show;
        }
        Thread downloadThread;
        TorrentItem torrent;
        TorrentHandle handle;
        Episode episode;
        Show show;
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
                addParams.SavePath = AppSettings.GetDownloadPath();
                addParams.Url = torrent.magnet;
                handle = session.AddTorrent(addParams);
                handle.SequentialDownload = sequential;
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => {
                    MainWindow.torrents.Add(new Tuple<TorrentHandle, Notification>(handle, null));
                }), DispatcherPriority.Send);
                while (true) {
                    var status = handle.QueryStatus();
                    if (status.IsSeeding) {
                        Tuple<TorrentHandle, Notification> t = MainWindow.torrents.Find(i => i.Item1 == handle);
                        MainWindow.torrents.Remove(t);
                        MainWindow.notifications.Remove(t.Item2);
                        DownloadFinished(handle);
                        break;
                    }
                    Thread.Sleep(2000);
                }
            }
        }
        private void DownloadFinished(TorrentHandle handle) {
            if (Directory.Exists(AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name)) {
                string path = AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name;
                List<string> files = Renamer.FilterExtensions(Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories).ToList());
                List<Episode> episodes = DatabaseEpisodes.ReadDb(show.id);
                int index = episodes.FindIndex(e => e.id == episode.id);
                episodes[index].downloaded = true;
                foreach (string file in files){
                    string output = Renamer.GetValidName(AppSettings.GetLibLocation()+"\\"+show.name, Renamer.GetName(show.name, episode.season, episode.episode, episode.name), Path.GetExtension(file), file);
                    File.Move(file, output);
                    episodes[index].locations.Add(output);
                }
                DatabaseEpisodes.CreateDB(show.id, episodes);
                Directory.Delete(path,true);
            } else if (File.Exists(AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name)){

            }
        }
    }
}
