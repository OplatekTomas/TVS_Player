using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TVS.API;

namespace TVSPlayer
{
    class UpdateDatabase {
        public async static Task Update() {
            if (Settings.LastCheck.AddDays(1).Date < DateTime.Now.Date) { 
                await Task.Run( () => {
                    List<int> ids = Series.GetUpdates(Settings.LastCheck);
                    List<Series> series = Database.GetSeries();
                    ids = ids.Where(x => series.Any(y => y.id == x)).ToList();
                    foreach (int id in ids) {
                        UpdateFullSeries(id);
                    }
                });
                await DownloadLastWeek();
                Settings.LastCheck = DateTime.Now;
            }
        }

        private async static Task DownloadLastWeek() {
            await Task.Run(async() => {
                var series = Database.GetSeries().Where(x=>x.autoDownload);
                var episodes = new Dictionary<Series,List<Episode>>();
                foreach (var se in series) {
                    //adds episodes that dont have files and have been released in last week
                    episodes.Add(se, Database.GetEpisodes(se.id).Where(x => x.files.Count == 0 && !String.IsNullOrEmpty(x.firstAired) && DateTime.ParseExact(x.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture) > DateTime.Now.AddDays(-7) && DateTime.ParseExact(x.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1) < DateTime.Now).ToList());
                }
                foreach (var combination in episodes) {
                    foreach (var episode in combination.Value) {
                        if (TorrentDatabase.Load().Where(x => x.Episode.id == episode.id).ToList().Count == 0) {
                            TorrentDownloader downloader = new TorrentDownloader(await Torrent.SearchSingle(combination.Key, episode, Settings.DownloadQuality));
                            await downloader.Download();
                        }
                    }

                }
            });
        }

        public async static Task CheckFiles() {
            await Task.Run(() => { 
                foreach (var series in Database.GetSeries()) {
                    foreach (var episode in Database.GetEpisodes(series.id)) {
                        for (int i = episode.files.Count - 1;i>=0;i--) {
                            if (!File.Exists(episode.files[i].NewName)) {
                                episode.files.RemoveAt(i);
                                Database.EditEpisode(series.id, episode.id, episode);
                            }
                        }
                    }
                }
            });
        }

        public async static void StartUpdateBackground() {
            await CheckFiles();
            await Update();
            Timer checktimer = new Timer(600000);
            checktimer.Elapsed += async (s, ev) => await CheckFiles();
            checktimer.Start();
            Timer timer = new Timer(3600000);
            timer.Elapsed += async (s, ev) => await Update();
            timer.Start();
        }



        private static void UpdateFullSeries(int id) {
            Task.Run(() => {
                UpdateSeries(id);
                UpdateEpisodes(id);
                UpdateEpisodes(id);
                UpdateActors(id);
            });

        }

        private static void UpdateSeries(int id) {
            Task.Run(() => {
                var series = Database.GetSeries(id);
                var newseries = Series.GetSeries(id);
                if (series.Compare(newseries)) {
                    series.Update(newseries);
                    Database.EditSeries(id, series);
                }
            });
        }

        private static void UpdateEpisodes(int id) {
            Task.Run(() => {
                var list = Episode.GetEpisodes(id);
                foreach (var episode in list) {
                    var ep = Database.GetEpisode(id, episode.id);
                    if (ep != null) {
                        if (ep.Compare(episode)) {
                            ep.Update(episode);
                            Database.EditEpisode(id, ep.id, ep);
                        }
                    } else {
                        Database.AddEpisode(id, episode);
                    }
                }
            });
        }

        private static void UpdateActors(int id) {
            Task.Run(() => {
                var list = Actor.GetActors(id);
                foreach (var actor in list) {
                    var ac = Database.GetActor(id, actor.id);
                    if (ac != null) {
                        if (ac.Compare(actor)) {
                            ac.Update(actor);
                            Database.EditActor(id, ac.id, ac);
                        }
                    } else {
                        Database.AddActor(id, actor);
                    }
                }

            });
        }

        private static void UpdatePosters(int id) {
            Task.Run(() => {
                var list = Poster.GetPosters(id);
                foreach (var poster in list) {
                    var po = Database.GetPoster(id,poster.id);
                    if (po != null) {
                        if (po.Compare(poster)) {
                            po.Update(poster);
                            Database.EditPoster(id, po.id, po);
                        }
                    } else {
                        Database.AddPoster(id, poster);
                    }
                }

            });
        }

    }
}
