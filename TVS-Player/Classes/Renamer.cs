using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player {
    class Renamer {
        public static string GetName(int id,string showName, int season, int episode) {
            string info = Api.apiGet(season,episode,id);
            JObject jo = JObject.Parse(info);
            string epName = jo["data"][0]["episodeName"].ToString();
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
        public static string GetValidName(string path, string extension,string name) {
            int filenumber = 1;
            string final;
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid) {
                name = name.Replace(c.ToString(), "");
            }
            do {
                final = path + "\\" + name + "_" + filenumber + extension;
                filenumber++;
            } while (File.Exists(path));
            return final;
        }
        public static List<string> ScanEpisodes(List<string> locations,int id){
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
            return showFiles;    
        }
        public static List<string> FilterExtensions(List<string> files) {
            string[] fileExtension = new string[10] { ".mkv", ".srt", ".m4v", ".avi", ".mp4", ".mov", ".sub", ".wmv", ".flv", ".idx" };
            List<string> filtered = new List<string>();
            foreach (string file in files) { 
                foreach (string ext in fileExtension) {
                    if (file.Contains(ext)) {
                        filtered.Add(file);
                        break;
                    }
                }
            }
            return filtered;
        }
        



    }
}
