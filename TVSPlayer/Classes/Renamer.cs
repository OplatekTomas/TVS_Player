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
        public List<Episode> FindEpisodes(TVShow show, List<Episode> episodes, string library, List<string> locations) {
            library = GetLibrary(library, show);
            List<string> allLocations = locations;
            allLocations.Add(library);
            List<string> files = new List<string>();
            
            foreach (string location in allLocations) {
                files.AddRange(SearchForFiles(location, show));
            }
            foreach (string file in files) {
                Tuple<int, int> info = GetSeasonEpisode(file);
                if (info != null) {
                    Episode episode = episodes.FirstOrDefault(e => e.season == info.Item1 && e.number == info.Item2);
                    if (episode != null) {
                        ScannedFile sf = Rename(file, library, show, episode);
                        
                    }
                }
            }
            return episodes;
        }

        public ScannedFile Rename(string file, string library,TVShow show, Episode episode) {
            ScannedFile sf = new ScannedFile();
            

        }



        private string GetLibrary(string library, TVShow show) {
            string libraryTemp = library;
            library = library + "\\" + show.seriesName;
            foreach (string alias in show.aliases) {
                if (Directory.Exists(library + "\\"+alias)) {
                    library = libraryTemp + "\\"+alias;
                }
            }
            return library;
        }

        private static Tuple<int, int> GetSeasonEpisode(string file) {
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

        /// <summary>
        /// Searches for files that are probably associated with a TV Show
        /// </summary>
        /// <param name="location">Where to scan</param>
        /// <param name="show">What to scan</param>
        /// <returns>list of strings that contain full path</returns>
        private static List<string> SearchForFiles(string location,TVShow show) {
            List<string> files = Directory.GetFiles(location, "*", SearchOption.AllDirectories).ToList();
            List<string> showFiles = new List<string>();
            foreach (string file in files) {
                foreach (string alias in show.aliases) {
                    if (Path.GetFileName(file).IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0 && !showFiles.Contains(file)) {
                        showFiles.Add(file);
                    }
                }
            }
            return FilterExtensions(showFiles);
        }
        /// <summary>
        /// Tries to filter extensions from list of files
        /// </summary>
        /// <param name="files">list of files</param>
        /// <returns>filtered list of filess</returns>
        private static List<string> FilterExtensions(List<string> files) {
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
