using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TVS.API;
using static TVS.API.Episode;

namespace TVSPlayer {
    class Renamer {

        /// <summary>
        /// Scans and renames direcotires (Lib, 3 scan dirs) and edits database for Series.
        /// </summary>
        /// <param name="series"></param>
        public static void FindAndRename(Series series) {
            List<ScannedFileInfo> temp = new List<ScannedFileInfo>();
            temp.AddRange(FindAndRenameInLibrary(series));
            temp.AddRange(FindAndRenameInOthers(series));
            foreach (ScannedFileInfo sfi in temp) {
                Database.EditEpisode(series.id, sfi.episode.id, sfi.episode);
            }
        }

        /// <summary>
        /// cans and renames direcotires (3 scan dirs) and edits database for Series.
        /// </summary>
        /// <param name="series"></param>
        public static void FindAndRenameInOther(Series series) {
            List<ScannedFileInfo> temp = FindAndRenameInOthers(series);
            foreach (ScannedFileInfo sfi in temp) {
                Database.EditEpisode(series.id, sfi.episode.id, sfi.episode);
            }
        }

        /// <summary>
        /// Called after torrent has finished downloading. Prefferably use StopAndMove() method from TorrentDownloader
        /// </summary>
        /// <param name="torrent">torrent downloader</param>
        public async static void MoveAfterDownload(TorrentDownloader torrent) {
            List<string> files = new List<string>();
            string path = torrent.Status.SavePath + "\\" + torrent.Status.Name;
            TorrentDownloader.TorrentSession.RemoveTorrent(torrent.Handle);
            bool moved = false;
            while (!moved) { 
                try {
                    if (File.Exists(path)) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "\\");
                        File.Move(path, Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "\\" + Path.GetFileName(path));
                        path = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + "\\";
                    }
                    var list = Rename(GetSeriesFilesInfo(torrent.TorrentSource.Series, path));
                    foreach (var item in list) {
                        Database.EditEpisode(torrent.TorrentSource.Series.id, item.episode.id, item.episode);
                    }
                    Directory.Delete(path, true);
                    moved = true;
                } catch (IOException e) {
                    await Task.Run(() => {
                        Thread.Sleep(100);
                    });
                }
            }
        }

        private static List<ScannedFileInfo> FindAndRenameInLibrary(Series series) {
            if (series.libraryPath == null) {
                CreateDirectoryForSeries(series);
                return new List<ScannedFileInfo>();
            } else {
                ScannedFile sf = new ScannedFile();
                List<ScannedFileInfo> files = GetSeriesFilesInfo(series, series.libraryPath);
                //files = GenerateNewPaths(files);
                return Rename(files);
            }
        }

        private static List<ScannedFileInfo> FindAndRenameInOthers(Series series) {
            List<ScannedFileInfo> files = new List<ScannedFileInfo>();
            if (Directory.Exists(Settings.FirstScanLocation)) files.AddRange(GetSeriesFilesInfo(series, Settings.FirstScanLocation));
            if (Directory.Exists(Settings.SecondScanLocation)) files.AddRange(GetSeriesFilesInfo(series, Settings.SecondScanLocation));
            if (Directory.Exists(Settings.ThirdScanLocation)) files.AddRange(GetSeriesFilesInfo(series, Settings.ThirdScanLocation));
            return Rename(files);
        }

        private static List<ScannedFileInfo> Rename(List<ScannedFileInfo> list) {
            List<ScannedFileInfo> newList = new List<ScannedFileInfo>();
            foreach (ScannedFileInfo sfi in list) {
                newList.Add(Rename(sfi).Result);
            }
            return newList;
        }

        private async static Task<ScannedFileInfo> Rename(ScannedFileInfo info) {
            info.newFile = GetPath(info);
            if (info.newFile != info.origFile) {
                try {
                    File.Move(info.origFile, info.newFile);
                } catch (IOException) {
                    MessageBoxResult result = await MessageBox.Show("File " + info.origFile + " is probably in use. \n\nTry again?", "Errror", MessageBoxButtons.YesNoCancel);
                    if (result == MessageBoxResult.Yes) {
                        return Rename(info).Result;
                    }
                }
            }
            bool add = true;
            if (info.episode.files.Count > 0) {
                foreach (ScannedFile sf in info.episode.files) {
                    if (sf.NewName == info.origFile) {
                        sf.NewName = info.newFile;
                        add = false;
                    }
                }
            }
            if (add) {
                info.episode.files.Add(Convert(info));
            }
            return info;
        }

        private static ScannedFile Convert(ScannedFileInfo info) {
            string[] subtitleTypes = new string[]{ ".srt", ".sub" };
            ScannedFile sf = new ScannedFile();
            sf.OriginalName = info.origFile;
            sf.NewName = info.newFile;
            sf.Type = ScannedFile.FileType.Video;
            foreach(string type in subtitleTypes) {
                if (info.extension == type) {
                    sf.Type = ScannedFile.FileType.Subtitles;
                }
            }
            return sf;
        }

        private static string GetPath(ScannedFileInfo info) {
            string name = GetName(info);
            string directory = null;
            int? season = info.episode.airedSeason;
            if (season < 10) {
                directory = info.series.libraryPath + @"\Season 0" + season+"\\";
                Directory.CreateDirectory(directory);
            } else if (season >= 10) {
                directory = info.series.libraryPath + @"\Season " + season+"\\";
                Directory.CreateDirectory(directory);
            }
            if (info.origFile == directory + name + info.extension) {
                return info.origFile;
            }
            string old = Path.GetFileNameWithoutExtension(info.origFile);
            Match match = new Regex(name + "_[0-9]?[0-9]").Match(old);
            if (match.Success) {
                if (!LowerAvailable(info.origFile, name)) {
                    return info.origFile;
                }
            }
            int filenumber = 1;
            string final = directory + name + info.extension;
            while (File.Exists(final)) {
                final = directory + name + "_" + filenumber + info.extension;
                filenumber++;
            }
            return final;
        }

        private static bool LowerAvailable(string file,string defaultNew) {
            int counter = 1;
            string path = Path.GetDirectoryName(file);
            string extension = Path.GetExtension(file);
            file = Path.GetFileNameWithoutExtension(file);
            string newFile = defaultNew;
            while (file != newFile) {
                if (!File.Exists(path + "\\" + newFile+"\\" + extension)) {
                    return true;
                }
                counter++;
                newFile = defaultNew + "_" + counter;
            }
            return false;
            
        }

        private static string GetName(ScannedFileInfo sfi) {
            string name = null;
            int? season = sfi.episode.airedSeason;
            int? episode = sfi.episode.airedEpisodeNumber;
            string epName = sfi.episode.episodeName;
            string showName = sfi.series.seriesName;
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
            return name;
        }

        private static List<ScannedFileInfo> GetSeriesFilesInfo(Series series, string path) {
            List<ScannedFileInfo> toRemove = new List<ScannedFileInfo>();
            List<Episode> allepisodes = Database.GetEpisodes(series.id);
            List<ScannedFileInfo> sfiList = ScanAndFilterFiles(series, path);
            foreach (ScannedFileInfo sfi in sfiList) {
                sfi.series = series;
                sfi.extension = Path.GetExtension(sfi.origFile);
                Tuple<int, int> info = GetInfo(Path.GetFileName(sfi.origFile));
                if (info != null) {
                    Episode episode = allepisodes.SingleOrDefault(e => e.airedSeason == info.Item1 && e.airedEpisodeNumber == info.Item2);
                    if (episode == null) {
                        toRemove.Add(sfi);
                    } else {
                        sfi.episode = episode;
                    }
                }               
            }
            foreach (ScannedFileInfo sfi in toRemove) {
                sfiList.Remove(sfi);
            }
            return sfiList;
        }

        private static Tuple<int, int> GetInfo(string file) {
            Match season = new Regex("[s][0-9][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match episode = new Regex("[e][0-9][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match special = new Regex("[0-9][0-9][x][0-9][0-9]", RegexOptions.IgnoreCase).Match(file);
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

        #region File filtering

        private static List<ScannedFileInfo> ScanAndFilterFiles(Series series, string path) {
            List<ScannedFileInfo> sfiList = new List<ScannedFileInfo>();
            List<string> files = FilterSeries(series,FilterExtensions(Directory.GetFiles(path, "*", SearchOption.AllDirectories).ToList()));
            foreach (string file in files) {
                ScannedFileInfo sfi = new ScannedFileInfo();
                sfi.origFile = file;
                sfiList.Add(sfi);
            }
            return sfiList;

        }
 
        private static List<string> FilterSeries(Series series, List<string> files) {
            List<string> newFiles = new List<string>();
            foreach (string file in files) {
                if (CheckAliases(file,series) || CheckAliasesParentDir(file,series)) {
                    newFiles.Add(file);
                }

            }
            return newFiles;
        }

        private static bool CheckAliasesParentDir(string path, Series series) {
            string pathDir = Path.GetFileName(Path.GetDirectoryName(path.ToUpper()));
            foreach (string alias in series.aliases) {
                string temp = alias.ToUpper();
                if ((pathDir.StartsWith(temp) || (pathDir.Contains("SEASON") && Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path.ToUpper()))).StartsWith(temp))) && IsMatchToIdentifiers(path)) {
                    return true;
                }
            }
            return false;
        }

        public static bool IsMatchToIdentifiers(string file) {
            Match season = new Regex("[s][0-9][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match episode = new Regex("[e][0-9][0-9]", RegexOptions.IgnoreCase).Match(file);
            Match special = new Regex("[0-5][0-9][x][0-5][0-9]", RegexOptions.IgnoreCase).Match(file);
            if ((season.Success && episode.Success && (episode.Index - season.Index) < 5) || special.Success) {
                return true;
            }
            return false;
        }

        private static bool CheckAliases(string file,Series series) {
            foreach (string alias in series.aliases) {
                if (Path.GetFileName(file.ToUpper()).StartsWith(alias.ToUpper())) {
                    return true;
                }
            }
            return false;
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

        #endregion
       
        private class ScannedFileInfo{
            public string origFile;
            public string newFile;
            public string extension;
            public Episode episode;
            public Series series;
        }

    }
}
