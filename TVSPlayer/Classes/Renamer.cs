using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TVS.API;
using static TVS.API.Episode;

namespace TVSPlayer {
    class Renamer {

        public static List<ScannedFile> FindAndRename(Series series) {
            List<ScannedFile> files = new List<ScannedFile>();
            files.AddRange(FindAndRenameLibrary(series));
            return files;
        }
        private static List<ScannedFile> FindAndRenameLibrary(Series series) {
            if (series.libraryPath == null) {
                CreateDirectoryForSeries(series);
                return new List<ScannedFile>();
            } else {
                List<Episode> allepisodes = Episode.GetAllEpisodes(series.id);
                ScannedFile sf = new ScannedFile();
                
                return new List<ScannedFile>();
            }
        }

        /// <summary>
        /// Creates directory for series. If directory exists it just adds _number
        /// </summary>
        /// <param name="series">What series to create directory for</param>
        private static void CreateDirectoryForSeries(Series series) {
            int i = 1;
            string path = Settings.Library + "\\" + series.seriesName;
            while (Directory.Exists(path)) {
                path = Settings.Library + "\\" + series.seriesName + "_" + i;
                i++;
            }
            Directory.CreateDirectory(path);
            series.libraryPath = path;
            Database.EditSeries(series.id, series); 
        }

        /// <summary>
        /// Returns name of an episode in format Series - SxxExx - Episode
        /// </summary>
        /// <param name="showName">name of Series</param>
        /// <param name="ep">Episode..</param>
        /// <returns>Series - SxxExx - Episode</returns>
        public static string GetName(string showName, Episode ep) {
            string name = null;
            int? season = ep.airedSeason;
            int? episode = ep.airedEpisodeNumber;
            string epName = ep.episodeName;
            if (season < 10) {
                if (episode < 10) {
                    name = showName + " - S0" + season + "E0" + episode + " - " + epName;
                } else if (episode >= 10) {
                    name = showName + " - S0" + season + "E" + episode + " - " + epName;
                }
            } else if (season >= 10) {
                if (episode < 10) {
                    name = showName + " - S" + season + "E0" + episode + " - " + epName;
                } else if (episode >= 10) {
                    name = showName + " - S" + season + "E" + episode + " - " + epName;
                }

            }
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid) {
                name = name.Replace(c.ToString(), "");
            }
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="series"></param>
        /// <param name="episode"></param>
        /// <returns></returns>
        private static string GetFilePath(Series series, Episode episode) {
            return null;
        }

        private static string GetPath(string path, string name, string extension) {
            int filenumber = 1;
            string final;
            Match m = new Regex("s[0-5][0-9]", RegexOptions.IgnoreCase).Match(name);
            int s = Int32.Parse(m.Value.Remove(0, 1));
            if (s < 10) {
                path += "\\Season 0" + s;
                Directory.CreateDirectory(path);
            } else if (s >= 10) {
                path += "\\Season " + s;
                Directory.CreateDirectory(path);
            }
            final = path + "\\" + name + extension;
            while (File.Exists(final)) {
                final = path + "\\" + name + "_" + filenumber + extension;
                filenumber++;
            }
            return final;
        }
        private static Tuple<int, int> GetInfo(string file) {
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

        private static List<string> FilterExtensions(List<string> files) {
            string[] fileExtension = new string[9] { ".mkv", ".srt", ".m4v", ".avi", ".mp4", ".mov", ".sub", ".wmv", ".flv" };
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
