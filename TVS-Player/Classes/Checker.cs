using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TVS_Player {
    class Checker {

        public static void RescanEP(Show show) {
            CheckExistence(show.id);
            List<string> showFiles = new List<string>();
            List<string> aliases = Api.GetAliases(show.id);
            List<string> files = new List<string>();
            foreach (string location in AppSettings.GetLocations()) {
                files.AddRange(Directory.GetFiles(location, "*.*", System.IO.SearchOption.AllDirectories));
            }
            foreach (string file in files) {
                foreach (string alias in aliases) {
                    if (Path.GetFileName(file).IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0 && !showFiles.Contains(file)) {
                        showFiles.Add(file);
                    }
                }
            }
            showFiles = Renamer.FilterExtensions(showFiles);
            string lib = AppSettings.GetLibLocation();
            AddFiles(showFiles, lib + "\\" + show.name, show.id, show.name);
        }

        private static void CheckExistence(int id) {
            List<Episode> list = DatabaseEpisodes.ReadDb(id);
            foreach (Episode e in list) {
                foreach (string file in e.locations.ToList()) {
                    if (!File.Exists(file)) {
                        e.locations.Remove(file);
                    }
                }
            }
            DatabaseEpisodes.CreateDB(id, list);
        }

        private static void AddFiles(List<string> files, string PathToShow, int id, string showName) {
            List<Episode> EPNames = DatabaseEpisodes.ReadDb(id);
            foreach (string file in files) {
                Tuple<int, int> t = Renamer.GetInfo(file);
                int season = t.Item1;
                int episode = t.Item2;
                var selectedEP = EPNames.FirstOrDefault(o => o.season == season && o.episode == episode);
                if (selectedEP == null) {
                    MessageBox.Show("This TV Show doesnt have episode " + episode + " in season " + season + ".\nFile " + file + " won't be renamed", "Error");
                } else {
                    string output = Renamer.GetValidName(PathToShow, Renamer.GetName(showName, selectedEP.season, selectedEP.episode, selectedEP.name), Path.GetExtension(file), file);
                    if (file != output) {
                        File.Move(file, output);
                        int ShowIndex = EPNames.FindIndex(e => e.season == season && e.episode == episode);
                        Episode ep = EPNames.Find(e => e.season == season && e.episode == episode);
                        List<string> tempPath = ep.locations;
                        tempPath.Add(output);
                        EPNames[ShowIndex] = new Episode(ep.name,ep.season,ep.episode,ep.id,ep.release,true,tempPath);
                    }
                }
            }
            DatabaseEpisodes.CreateDB(id,EPNames);
        }

       
        public static void Update(int id) {
            List<Episode> EPList = DatabaseEpisodes.ReadDb(id);
            int season = EPList.Max(y => y.season);
            string seasonInfo = Api.apiGetEpisodesBySeasons(id, season);
            string nextSeason = Api.apiGetEpisodesBySeasons(id, season+1);
            if (nextSeason != null) {
                JObject jo = JObject.Parse(nextSeason);
                foreach (JToken jt in jo["data"]) {
                    string time;
                    if (jt["firstAired"].ToString() != "") {
                        time = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd.MM.yyyy");
                    } else {
                        time = "--.--.----";
                    }
                    EPList.Add(new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), time , false, new List<string>()));
                }
            }
            JObject jo2 = JObject.Parse(seasonInfo);
            foreach (JToken jt in jo2["data"]) {
                string time;
                if (jt["firstAired"].ToString() != "") {
                    time = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd.MM.yyyy");
                } else {
                    time = "--.--.----";
                }
                int episode = Int32.Parse(jt["airedEpisodeNumber"].ToString());
                int index = EPList.FindIndex(s =>s.season == season && s.episode == episode);
                if (index != -1) {
                    EPList[index] = new Episode(jt["episodeName"].ToString(), season, episode, Int32.Parse(jt["id"].ToString()), time, EPList[index].downloaded, EPList[index].locations);
                } else {
                    EPList.Add(new Episode(jt["episodeName"].ToString(), season, episode, Int32.Parse(jt["id"].ToString()), time, EPList[index].downloaded, EPList[index].locations));
                }

            }
            DatabaseEpisodes.CreateDB(id,EPList);
        }

        public static void UpdateFull(int id) {
            List<Episode> epi = DatabaseEpisodes.ReadDb(id);
            for (int i = 1; i <= Renamer.GetNumberOfSeasons(id); i++) {
                JObject jo = JObject.Parse(Api.apiGetEpisodesBySeasons(id, i));
                foreach (JToken jt in jo["data"]) {
                    string time;
                    if (jt["firstAired"].ToString() != "") {
                        DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        time = dt.ToString("dd.MM.yyyy");
                    } else {
                        time = ".--.----";
                    }
                    int episode = Int32.Parse(jt["airedEpisodeNumber"].ToString());
                    try {
                        int index = epi.FindIndex(s => s.season == i && s.episode == episode);
                        epi[index] = new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()),time , epi[index].downloaded, epi[index].locations);
                    } catch (Exception) {
                        epi.Add(new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()),time, false, new List<string>()));
                    }
                }
            }
            DatabaseEpisodes.CreateDB(id, epi);
        }

    }
}
