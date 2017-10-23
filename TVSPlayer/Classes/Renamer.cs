using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<ScannedFile> result = new List<ScannedFile>();
            List<ScannedFileInfo> temp = new List<ScannedFileInfo>();
            temp.AddRange(FindAndRenameInLibrary(series));
            temp.AddRange(FindAndRenameInOthers(series));
            sw.Stop();
            return result;
        }


        private static List<ScannedFileInfo> FindAndRenameInLibrary(Series series) {
            if (series.libraryPath == null) {
                CreateDirectoryForSeries(series);
                return new List<ScannedFileInfo>();
            } else {
                ScannedFile sf = new ScannedFile();
                List<ScannedFileInfo> files = GetSeriesFilesInfo(series, series.libraryPath);
                files = GenerateNewPaths(files);
                return files;
            }
        }

        private static List<ScannedFileInfo> GenerateNewPaths(List<ScannedFileInfo> list) {
            foreach (ScannedFileInfo item in list) {
                item.newFile = GetPath(item, GetName(item));
            }
            return list;
        }

        private static string GetPath(ScannedFileInfo info, string name) {
            string directory = null;
            int? season = info.episode.airedSeason;
            int filenumber = 1;
            if (season < 10) {
                directory = info.series.libraryPath + @"\Season 0" + season;
                Directory.CreateDirectory(directory);
            } else if (season >= 10) {
                directory = info.series.libraryPath + @"\Season " + season;
                Directory.CreateDirectory(directory);
            }
            if (info.origFile == directory + name + info.extension) {
                return info.origFile;
            }
            string old = Path.GetFileNameWithoutExtension(info.origFile);
            Match match = new Regex(GetName(info) + "_[0-9]?[0-9]").Match(old);
            if (match.Success) {
                if (!LowerAvailable()) {

                }
            }

            final = path + "\\" + name + extension;
            while (File.Exists(final)) {
                final = path + "\\" + name + "_" + filenumber + extension;
                filenumber++;
            }
            return final;
        }

        private bool LowerAvailable(string file,string defaultNew) {
            int counter = 1;
            if(File.Exists())
            while
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



        private static List<ScannedFileInfo> FindAndRenameInOthers(Series series) {
            List<ScannedFileInfo> files = new List<ScannedFileInfo>();
            if(Directory.Exists(Settings.FirstScanLocation)) files.AddRange(GetSeriesFilesInfo(series, Settings.FirstScanLocation));
            if (Directory.Exists(Settings.SecondScanLocation)) files.AddRange(GetSeriesFilesInfo(series, Settings.SecondScanLocation));
            if (Directory.Exists(Settings.ThirdScanLocation)) files.AddRange(GetSeriesFilesInfo(series, Settings.ThirdScanLocation));
            return files;
        }

        private static List<ScannedFileInfo> GetSeriesFilesInfo(Series series, string path) {
            List<ScannedFileInfo> toRemove = new List<ScannedFileInfo>();
            List<Episode> allepisodes = Episode.GetAllEpisodes(series.id);
            List<ScannedFileInfo> sfiList = ScanAndFilterFiles(series, path);
            foreach (ScannedFileInfo sfi in sfiList) {
                sfi.series = series;
                sfi.extension = Path.GetExtension(sfi.origFile);
                Tuple<int, int> info = GetInfo(Path.GetFileName(sfi.origFile));
                Episode episode = allepisodes.SingleOrDefault(e => e.airedSeason == info.Item1 && e.airedEpisodeNumber == info.Item2);
                if (episode == null) {
                    toRemove.Add(sfi);
                } else {
                    sfi.episode = episode;
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

        private static bool IsMatchToIdentifiers(string file) {
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


        private class ScannedFileInfo{
            public string origFile;
            public string newFile;
            public string extension;
            public Episode episode;
            public Series series;
        }

    }
}
