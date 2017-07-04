using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TVSPlayer.Episode;

namespace TVSPlayer {
    class Renamer {
        public static List<Episode> FindAndMoveEpisodes(TVShow show, List<Episode> episodes, string library, List<string> locations) {
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

        /// <summary>
        /// Moves file from somewhere to library + Season XX
        /// </summary>
        /// <param name="file">What file to move</param>
        /// <param name="library">Where to move the file</param>
        /// <param name="show">What TVShow is the file</param>
        /// <param name="episode">What episode is the file</param>
        /// <returns>ScannedFile. Everything is filled. If this file already is in database it will return edited instance of the original</returns>
        public static ScannedFile Rename(string file, string library,TVShow show, Episode episode) {
            ScannedFile sf = new ScannedFile();
            bool moved = false;
            string newFile = getNewFileName(file, library, show, episode);
            string defaultName = getDefaultNewFileName(file, library, show, episode);
            //Check if file is already one of correctly named files in library
            bool check = false;
            for (int counter = 1; counter < 10; counter++) {
                if (file == Path.ChangeExtension(defaultName, null) + "_"+ counter + Path.GetExtension(defaultName)) {
                    check = true;
                }
            }
            if (check || file == defaultName) {
                if (episode.files != null && episode.files.Count > 0) {
                    ScannedFile search = episode.files.FirstOrDefault(f => f.path == file);
                    if (search == null) {
                        sf.origPath = null;
                        sf.path = newFile;
                        sf.type = GetFileType(file);
                    } else {
                        sf.origPath = search.origPath;
                        sf.path = newFile;
                        sf.type = search.type;
                    }
                } else {
                    sf.origPath = file;
                    sf.path = newFile;
                    sf.type = GetFileType(file);
                }
            } else {
                sf.origPath = file;
                sf.path = newFile;
                sf.type = GetFileType(file);               
            }
            while (!moved) {
                try {
                    File.Move(file, newFile);
                    moved = true;
                } catch (Exception e) {
                    MessageBox.Show(e.Message);
                }
            }
            return sf;
        }

        private static ScannedFile.FileType? GetFileType(string file) {
            string[] video = new string[7] { ".mkv", ".m4v", ".avi", ".mp4", ".mov", ".wmv", ".flv"};
            string[] subs = new string[3] {".srt", ".sub", ".idx" };
            string ext = Path.GetExtension(file);
            if (video.Contains(ext,StringComparer.CurrentCultureIgnoreCase)) {
                return ScannedFile.FileType.Video;
            }
            if (subs.Contains(ext, StringComparer.CurrentCultureIgnoreCase)) {
                return ScannedFile.FileType.Subtitle;
            }
            return null;

        }

        private static string getDefaultNewFileName(string file, string library, TVShow show, Episode episode) {
            if (episode.season >= 10) {
                library += "\\Season " + episode.season;
            } else {
                library += "\\Season 0" + episode.season;
            }
            if (!Directory.Exists(library)) {
                Directory.CreateDirectory(library);
            }
            return library + "\\" + episode.GetName(show) + Path.GetExtension(file);
        }

        private static string getNewFileName(string file,string library, TVShow show, Episode episode) {
            if (episode.season >= 10) {
                library += "\\Season " + episode.season; 
            } else {
                library += "\\Season 0" + episode.season;
            }
            if (!Directory.Exists(library)) {
                Directory.CreateDirectory(library);
            }
            bool passed = false;
            int counter = 1;
            string newFile = library + "\\" + episode.GetName(show) + Path.GetExtension(file);
            while (!passed) {
                if (!File.Exists(newFile)) {
                    passed = true;
                } else {
                    newFile = library + "\\" + episode.GetName(show) + "_" + counter + Path.GetExtension(file);
                    counter++;
                }
            }
            return newFile;
        }


        private static string GetLibrary(string library, TVShow show) {
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
