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
            Action a = () => Down();
            downloadThread = new Thread(a.Invoke);
            downloadThread.IsBackground = true;
            downloadThread.Name = "Torrent download";
            downloadThread.Start();
        }

        private void Down() {
            string tempFile = null;
            bool added = false;
            using (var session = new Session()) {
                session.ListenOn(6881, 6889);               
                var addParams = new AddTorrentParams();
                addParams.SavePath = AppSettings.GetDownloadPath();
                addParams.Url = torrent.magnet;
                handle = session.AddTorrent(addParams);
                handle.SequentialDownload = AppSettings.GetSeqDownload();
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
                    if (handle.SequentialDownload == true) {
                        if (status.TotalDownload > 5 && !added) {
                            //tempFile = getFile();
                            //AddTempToDb(tempFile);
                            added = true;
                        }
                    }

                    Thread.Sleep(2000);
                }
            }
        }

        private string getFile() {
            string path = AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name;
            if (Directory.Exists(path)) {
                List<string> files = Renamer.FilterExtensionsVideo(Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories).ToList());
                string biggest = files[0];
                long length = new FileInfo(files[0]).Length;
                if (files.Count > 1) { 
                    for (int i = 0; i < files.Count;i++) {
                        long lengthFirst = new FileInfo(files[i]).Length;
                        if (lengthFirst > length) {
                            biggest = files[i];
                        }
                    }
                }
                return biggest;
            } else if (File.Exists(path)) {
                return path;            
            }
            return null;
        }

        private void AddTempToDb(string path) {
            List<Episode> episodes = DatabaseEpisodes.ReadDb(show.id.TVDb);
            int index = episodes.FindIndex(e => e.id == episode.id);
            episodes[index].downloaded = true;
            episodes[index].locations.Add(path);
            DatabaseEpisodes.CreateDB(show.id.TVDb, episodes);
        }

        private void DownloadFinished(TorrentHandle handle,string pathTemp = null) {
            Thread.Sleep(3000);
            List<Episode> episodes = DatabaseEpisodes.ReadDb(show.id.TVDb);
            int index = episodes.FindIndex(e => e.id == episode.id);
            episodes[index].downloaded = true;
            /*if (handle.SequentialDownload == true) {
                episodes[index].locations.Remove(pathTemp);
            }*/
            if (Directory.Exists(AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name)) {
                string path = AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name;
                List<string> files = Renamer.FilterExtensions(Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories).ToList());
                foreach (string file in files){
                    string output = Renamer.GetValidName(AppSettings.GetLibLocation()+"\\"+show.name, Renamer.GetName(show.name, episode.season, episode.episode, episode.name), Path.GetExtension(file), file);
                    File.Move(file, output);
                    episodes[index].locations.Add(output);
                }
                DatabaseEpisodes.CreateDB(show.id.TVDb, episodes);
                Directory.Delete(path,true);
            } else if (File.Exists(AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name)){
                string path = AppSettings.GetDownloadPath() + "\\" + handle.TorrentFile.Name;
                string output = Renamer.GetValidName(AppSettings.GetLibLocation() + "\\" + show.name, Renamer.GetName(show.name, episode.season, episode.episode, episode.name), Path.GetExtension(path), path);
                File.Move(path, output);
                episodes[index].locations.Add(output);
                DatabaseEpisodes.CreateDB(show.id.TVDb, episodes);
            }
        }
    }
}
