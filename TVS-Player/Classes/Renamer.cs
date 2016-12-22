using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TVS_Player {
    public class EPInfo {
        public string name;
        public int season;
        public int episode;
        public EPInfo(string n, int s, int e) {
            name = n;
            season = s;
            episode = e;
        }
    }


    class Renamer {

        public static string GetName(string showName, int season, int episode,string epName) {
            if (season < 10) {
                if (episode < 10) {
                    return showName + " - S0" + season + "E0" + episode + " - " + epName;
                } else if (episode >= 10) {
                    return showName + " - S0" + season + "E" + episode + " - " + epName;
                }
            } else if (season >= 10) {
                if (episode < 10) {
                    return showName + " - S" + season + "E0" + episode + " - " + epName;
                } else if (episode >= 10) {
                    return showName + " - S" + season + "E" + episode + " - " + epName;
                }

            }
            return null;
        }

        public static string GetValidName(string path, string name, string extension) {
            int filenumber = 1;
            string final;
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Match m = new Regex("s[0-5][0-9]", RegexOptions.IgnoreCase).Match(name);
            int s = Int32.Parse(m.Value.Remove(0, 1));
            foreach (char c in invalid) {
                name = name.Replace(c.ToString(), "");
            }
            if (s < 10) {               
                path += "\\Season 0" + s;
                Directory.CreateDirectory(path);
            } else if (s >= 10) {
                path += "\\Season " + s;
                Directory.CreateDirectory(path);                
            }
            final = path + "\\" + name + extension;
            while (File.Exists(final)){
                final = path + "\\" + name + "_" + filenumber + extension;
                filenumber++;
            }
            return final;
        }
        public static void RenameBatch(List<int> ids,List<string> scan,string lib) {
            foreach (int id in ids) {
                string showName = Api.getName(id);
                List<string> files = ScanEpisodes(scan,id);
                Directory.CreateDirectory(lib + "\\" +showName);
                RenameFiles(files, lib+"\\"+showName, id, showName);

            }

        }


        public static List<string> ScanEpisodes(List<string> locations, int id) {
            List<string> showFiles = new List<string>();
            List<string> aliases = Api.GetAliases(id);
            foreach (string location in locations) {
                List<string> files = new List<string>();
                files.AddRange(Directory.GetFiles(location, "*.*", System.IO.SearchOption.AllDirectories));
                foreach (string file in files) {
                    foreach (string alias in aliases) {
                        if (Path.GetFileName(file).IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0 && !showFiles.Contains(file)) {
                            showFiles.Add(file);
                        }
                    }
                }
            }
            return FilterExtensions(showFiles);
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

        public static void RenameFiles(List<string> files, string path, int id, string showName) {
            List<EPInfo> EPNames = DownloadInfo(id);
            foreach (string file in files) {
                Tuple<int, int> t = GetInfo(file);
                int season = t.Item1;
                int episode = t.Item2;
                var selectedEP = EPNames.FirstOrDefault(o => o.season == season && o.episode == episode);
                if (selectedEP == null) {
                    MessageBox.Show("This TV Show doesnt have episode "+ episode + " in season " + season+".\nFile " + file + " won't be renamed", "Error");
                } else {
                    string output = GetValidName(path, GetName(showName, selectedEP.season, selectedEP.episode, selectedEP.name), Path.GetExtension(file));
                    File.Move(file, output); 
                }
            }


        }
        public static List<EPInfo> DownloadInfo(int id) {
            List<EPInfo> epi = new List<EPInfo>();
            for (int i = 1; i <= GetNumberOfSeasons(id); i++) { 
                JObject jo = JObject.Parse(Api.apiGetEpisodesBySeasons(id, i));
                foreach (JToken jt in jo["data"]) {                 
                    epi.Add(new EPInfo(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString())));
                }          
            }
            return epi;
        }
        public static int GetNumberOfSeasons(int id) {
            JObject s = JObject.Parse(Api.apiGetSeasons(id));
            int count = s["data"]["airedSeasons"].Count();
            foreach (JToken jt in s["data"]["airedSeasons"]) {
                if (jt.Value<int>() == 0) {
                    count--;
                }
            }
            return count;
        }
        public static void RenameSingle(string file, string lib, int id) {
            string showName = Api.getName(id);
            Tuple<int, int> t = GetInfo(file);
            int season = t.Item1;
            int episode = t.Item2;
            string epName = JObject.Parse(Api.apiGet(season, episode, id))["data"][0]["episodeName"].ToString();
            try {
                File.Move(file, GetValidName(lib, GetName(showName, season, episode, epName), Path.GetExtension(file)));
            } catch (Exception) {
                MessageBox.Show("File couldn't be moved");
            }
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

    }
}
