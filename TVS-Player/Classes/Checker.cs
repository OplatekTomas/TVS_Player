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
        public static void UpdateShow(int id) {
            Action check;
            check = () => Update(id);
            Thread threadCheck = new Thread(check.Invoke);
            threadCheck.Name = "Checking for updates";
            threadCheck.Start();
        }
        public static void UpdateShowFull(int id) {
            Action check;
            check = () => UpdateFull(id);
            Thread threadCheck = new Thread(check.Invoke);
            threadCheck.Name = "Checking for updates";
            threadCheck.Start();
        }

        private static void RescanEP(int id, List<string> locations) {
            List<string> showFiles = new List<string>();
            List<string> aliases = Api.GetAliases(id);
            List<string> files = new List<string>();
            foreach (string location in locations) {
                files.AddRange(Directory.GetFiles(location, "*.*", System.IO.SearchOption.AllDirectories));
            }
            foreach (string file in files) {
                foreach (string alias in aliases) {
                    if (Path.GetFileName(file).IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0 && !showFiles.Contains(file)) {
                        showFiles.Add(file);
                    }
                }
            }
            showFiles = FilterExtensions(showFiles);
            string showName = DatabaseAPI.database.Shows.Find(s => Int32.Parse(s.idSel)== id).nameSel;
            string lib = DatabaseAPI.database.libraryLocation;
            RenameFiles(files, lib + "\\" + showName, id, showName);

        }

        public static void RenameFiles(List<string> files, string path, int id, string showName) {
            List<Episode> EPNames = new List<Episode>(); 
            foreach (string file in files) {
                Tuple<int, int> t = GetInfo(file);
                int season = t.Item1;
                int episode = t.Item2;
                var selectedEP = EPNames.FirstOrDefault(o => o.season == season && o.episode == episode);
                int index = EPNames.FindIndex(o => o.season == season && o.episode == episode);
                if (selectedEP == null) {
                    MessageBox.Show("This TV Show doesnt have episode " + episode + " in season " + season + ".\nFile " + file + " won't be renamed", "Error");
                } else {
                    string output = Renamer.GetValidName(path, Renamer.GetName(showName, selectedEP.season, selectedEP.episode, selectedEP.name), Path.GetExtension(file), file);
                    if (file != output) {
                        File.Move(file, output);
                        int ShowIndex = DatabaseAPI.database.Shows.FindIndex(e => Int32.Parse(e.idSel) == id);
                    }
                    EPNames[index].downloaded = true;
                    EPNames[index].locations.Add(output);
                }
            }
        }

        public static Tuple<int, int> GetInfo(string file) {
            Match season = new Regex("[s][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match episode = new Regex("[e][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match special = new Regex("[0-5][0-9][x][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            if (season.Success && episode.Success) {
                int s = Int32.Parse(season.Value.Remove(0, 1));
                int e = Int32.Parse(episode.Value.Remove(0, 1));
                return new Tuple<int, int>(s, e);
            } else if (special.Success) {
                int s = Int32.Parse(special.Value.Substring(0, 2));
                int e = Int32.Parse(special.Value.Substring(3, 2));
                return new Tuple<int, int>(s, e);
            }
            return null;
        }
        public static List<string> FilterExtensions(List<string> files) {
            string[] fileExtension = new string[10] { ".mkv", ".srt", ".m4v", ".avi", ".mp4", ".mov", ".sub", ".wmv", ".flv", ".idx" };
            List<string> filtered = new List<string>();
            foreach (string file in files) {
                if (fileExtension.Any(file.Contains)) {
                    filtered.Add(file);
                }
            }
            return filtered;
        }
        private static void Update(int id) {
            List<Episode> EPList = DatabaseEpisodes.readDb(id);
            int season = EPList.Max(y => y.season);
            string seasonInfo = Api.apiGetEpisodesBySeasons(id, season);
            string nextSeason = Api.apiGetEpisodesBySeasons(id, season+1);
            if (nextSeason != null) {
                JObject jo = JObject.Parse(nextSeason);
                foreach (JToken jt in jo["data"]) {
                    DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-mm-dd", CultureInfo.InvariantCulture);
                    EPList.Add(new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), dt.ToString("dd.mm.yyyy"), false, new List<string>()));
                }
            }
            JObject jo2 = JObject.Parse(seasonInfo);
            foreach (JToken jt in jo2["data"]) {
                DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                int episode = Int32.Parse(jt["airedEpisodeNumber"].ToString());
                int index = EPList.FindIndex(s =>s.season == season && s.episode == episode);            
                EPList[index] = new Episode(jt["episodeName"].ToString(),season,episode, Int32.Parse(jt["id"].ToString()), dt.ToString("dd.MM.yyyy"), EPList[index].downloaded, EPList[index].locations);
            }
            DatabaseEpisodes.createDB(id,EPList);
        }
        private static void UpdateFull(int id) {
            List<Episode> epi = DatabaseEpisodes.readDb(id);
            for (int i = 1; i <= Renamer.GetNumberOfSeasons(id); i++) {
                JObject jo = JObject.Parse(Api.apiGetEpisodesBySeasons(id, i));
                foreach (JToken jt in jo["data"]) {
                    DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-mm-dd", CultureInfo.InvariantCulture);
                    int episode = Int32.Parse(jt["airedEpisodeNumber"].ToString());
                    try {
                        int index = epi.FindIndex(s => s.season == i && s.episode == episode);
                        epi[index] = new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), dt.ToString("dd.mm.yyyy"), epi[index].downloaded, epi[index].locations);
                    } catch (ArgumentNullException) {
                        epi.Add(new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), dt.ToString("dd.mm.yyyy"), false, new List<string>()));
                    }
                }
            }
            DatabaseEpisodes.createDB(id, epi);
        }

    }
}
