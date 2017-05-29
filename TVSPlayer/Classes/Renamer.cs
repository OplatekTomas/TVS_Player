using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TVSPlayer.Episode;

namespace TVSPlayer {
    class Renamer {
        /// <summary>
        /// Scans and renames all episodes it finds in parameter locations then moves them to databaseLocation
        /// </summary>
        /// <param name="locations">Locations that will be scanned for episodes</param>
        /// <param name="databaseLocation">Location where episodes are saved (seriesName is added)</param>
        /// <param name="show">Yeah you need that.</param>
        /// <param name="episodes">Important. if episode is not supplied it won't find anything</param>
        /// <returns>Original list of episodes, but ScannedFile</returns>
        public static List<Episode> RenameBatch(List<string> locations, string databaseLocation, TVShow show,List<Episode> episodes) {
            locations.Insert(0, databaseLocation);
            List<string> videos = new List<string>() { ".mkv",  ".m4v" ,".avi", ".mp4", ".mov",  ".wmv", ".flv" };
            List<string> subs = new List<string>() { ".sub", ".srt", ".idx" };
            string databaseL = databaseLocation;
            databaseLocation = databaseLocation + "\\" + show.seriesName;
            foreach (string alias in show.aliases) {
                if (Directory.Exists(databaseLocation + "\\"+alias)) {
                    databaseLocation = databaseL + "\\"+alias;
                }
            }
            List<string> files = ScanEpisodes(locations, show);
            foreach (string file in files) {
                Tuple<int, int> t = GetInfo(file);
                if (t != null) {
                    int season = t.Item1;
                    int episode = t.Item2;
                    Episode selectedEP = episodes.FirstOrDefault(o => o.season == season && o.number == episode);
                    int index = episodes.FindIndex(o => o.season == season && o.number == episode);
                    if (selectedEP != null) {
                        ScannedFile sf = RenameFile(file, databaseLocation, show, selectedEP);
                        string ext = Path.GetExtension(file);
                        if (videos.Contains(ext)) { sf.type = ScannedFile.FileType.Video; } else if(subs.Contains(ext)) { sf.type = ScannedFile.FileType.Subtitle; }
                        int idx = selectedEP.files.FindIndex(s => s.origPath == sf.path);
                        if (idx == -1) {
                            selectedEP.files.Add(sf);
                        }

                    }
                }
            }
            return episodes;
        }


        private static string GetValidName(string path, string name, string extension, string original, int s) {
            int filenumber = 1;
            string final;
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
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
            if (original != final) {
                while (File.Exists(final)) {
                    final = path + "\\" + name + "_" + filenumber + extension;
                    filenumber++;
                }
            }
            return final;
        }
        private static List<string> ScanEpisodes(List<string> locations, TVShow s) {
            List<string> showFiles = new List<string>();
            List<string> files = new List<string>();
            foreach (string location in locations) {
                if (Directory.Exists(location)) {
                    files.AddRange(Directory.GetFiles(location, "*.*", System.IO.SearchOption.AllDirectories));
                }
            }
            foreach (string file in files) {
                foreach (string alias in s.aliases) {
                    if (Path.GetFileName(file).IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0 && !showFiles.Contains(file)) {
                        showFiles.Add(file);
                    }
                }
            }
            return FilterExtensions(showFiles);
        }
        private static Tuple<int, int> GetInfo(string file) {
            file = Path.GetFileName(file);
            Match season = new Regex("[s][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match episode = new Regex("[e][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match special = new Regex("[0-5][0-9][x][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            if (season.Success && episode.Success && (episode.Index - season.Index) < 5) {
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
        private static List<string> FilterExtensions(List<string> files) {
            string[] fileExtension = new string[10] { ".mkv", ".srt", ".m4v", ".avi", ".mp4", ".mov", ".sub", ".wmv", ".flv",".idx" };
            List<string> filtered = new List<string>();
            foreach (string file in files) {
                if (fileExtension.Any(file.Contains)) {
                    filtered.Add(file);
                }
            }
            return filtered;
        }
        private static long GetDirectorySize(string parentDirectory) {
            return new DirectoryInfo(parentDirectory).GetFiles("*.*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        private static ScannedFile RenameFile(string file, string path, TVShow show, Episode epi) {
            ScannedFile sf = new ScannedFile();
            string output = GetValidName(path, epi.GetName(show), Path.GetExtension(file), file, epi.season);
            sf.path = output;
            
            if (file != output) {
                sf.origPath = file.Split('\\').Last();
                bool moved = false;
                do {
                    try {
                        File.Move(file, output);
                        moved = true;
                    } catch (Exception e) {
                        DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("File " + file + " couldn't be renamed.\nClose apps that might be using it.\n\nTry again?", "Error", MessageBoxButtons.YesNo);
                        System.Windows.Forms.MessageBox.Show(e.Message);
                        if (dialogResult == DialogResult.No) {
                            moved = true;
                        }
                    }
                } while (moved != true);
            }
            return sf;
        }
       
    }
}
