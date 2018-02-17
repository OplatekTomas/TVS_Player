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
using System.Windows.Threading;
using TVS.API;
using static TVS.API.Episode;

namespace TVSPlayer {
    class Renamer {

        Episode Episode { get; set; }
        List<ScannedFileInfo> Files { get; set; }

        private Renamer(Episode episode) {
            Episode = episode;
            Files = new List<ScannedFileInfo>();
        }

        public static async Task ScanAndRename(List<Series> list) {
            await Task.Run( async () => {
                var files = GetFilesInScanDirs();
                foreach (var series in list) {
                    var current = GetFilesInLibrary(series);
                    current.AddRange(files);
                    var filesWithInfo = GetBasicInfo(series, current);
                    foreach (var info in filesWithInfo) {
                        foreach (var file in info.Files) {
                            var result = await Rename(file);
                            info.Episode = AddToDatabase(info.Episode, result);
                        }
                        Database.EditEpisode(series.id, info.Episode.id, info.Episode);
                    }
                    var allFiles = filesWithInfo.SelectMany(f => f.Files);
                    var filter = allFiles.Where(f => !f.fromLibrary);
                    files.RemoveAll(x => filter.Any(y => y.origFile == x.origFile));
                }
            });
        }

        public async static Task RenameAfterDownload(TorrentDownloader torrent) {
            await Task.Run(async () => {
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
                        var files = FilterSeries(torrent.TorrentSource.Series, GetFilesInDirectory(path));
                        var episode = Database.GetEpisode(torrent.TorrentSource.Series.id, torrent.TorrentSource.Episode.id);
                        files.ForEach(x => x.episode = episode);
                        foreach (var item in files) {
                            var result = await Rename(item);
                            episode = AddToDatabase(episode, result);
                        }
                        Database.EditEpisode(torrent.TorrentSource.Series.id, episode.id, episode);
                        Directory.Delete(path, true);
                        moved = true;
                    } catch (IOException e) {
                       await Task.Delay(100);
                    }
                }
            });
           
        }

        public async static Task RenameSingle(string file, Series series, Episode episode) {
            var sfi = new ScannedFileInfo();
            sfi.origFile = file;            
            sfi.extension = Path.GetExtension(file);
            sfi.series = series;
            sfi.episode = episode;
            sfi.type = GetFileType(file);
            var result = await Rename(sfi);
            episode = AddToDatabase(episode, sfi);
            Database.EditEpisode(series.id, episode.id, episode);
        }

        private static Episode AddToDatabase(Episode episode, ScannedFileInfo sfi) {
            bool add = true;
            if (episode.files.Count > 0) {
                foreach (ScannedFile sf in episode.files) {
                    if (sf.NewName == sfi.origFile) {
                        sf.NewName = sfi.newFile;
                        add = false;
                    }
                }
            }
            if (add) {
                episode.files.Add(Convert(sfi));
            }
            return episode;
        }

        private static ScannedFile Convert(ScannedFileInfo info) {
            ScannedFile sf = new ScannedFile();
            sf.OriginalName = info.origFile;
            sf.NewName = info.newFile;
            sf.Type = info.type;
            return sf;
        }

        private async static Task<ScannedFileInfo> Rename(ScannedFileInfo info) {
            info.newFile = GetPath(info);
            if (info.newFile != info.origFile) {
                try {
                    File.Move(info.origFile, info.newFile);
                } catch (IOException) {
                    MessageBoxResult result = MessageBoxResult.Cancel;
                    await Application.Current.Dispatcher.Invoke(async () => {
                        result = await MessageBox.Show("File " + info.origFile + " is probably in use. \n\nTry again?", "Error", MessageBoxButtons.YesNoCancel);
                    });
                    if (result == MessageBoxResult.Yes) {
                        return await Rename(info);
                    }
                }
            }
            return info;
        }

        private static string GetPath(ScannedFileInfo info) {
            string name = Helper.GenerateName(info.series, info.episode);
            string directory = null;
            int? season = info.episode.airedSeason;
            if (season < 10) {
                directory = info.series.libraryPath + @"\Season 0" + season + "\\";
                Directory.CreateDirectory(directory);
            } else if (season >= 10) {
                directory = info.series.libraryPath + @"\Season " + season + "\\";
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

        private static bool LowerAvailable(string file, string defaultNew) {
            int counter = 1;
            string path = Path.GetDirectoryName(file);
            string extension = Path.GetExtension(file);
            file = Path.GetFileNameWithoutExtension(file);
            string newFile = defaultNew;
            while (file != newFile) {
                if (!File.Exists(path + "\\" + newFile + "\\" + extension)) {
                    return true;
                }
                counter++;
                newFile = defaultNew + "_" + counter;
            }
            return false;

        }

        private static List<Renamer> GetBasicInfo(Series series,List<ScannedFileInfo> list) {
            var dictionary = new List<Renamer>();
            Database.GetEpisodes(series.id).ForEach(x => dictionary.Add(new Renamer(x)));            
            list = FilterSeries(series, list);
            foreach (var file in list) {
                var info = GetInfo(file);
                if (info != null) {
                    var episode = dictionary.FirstOrDefault(x => x.Episode.airedEpisodeNumber == info.Item2 && x.Episode.airedSeason == info.Item1);
                    if (episode != null) {
                        file.episode = episode.Episode;
                        dictionary[dictionary.IndexOf(episode)].Files.Add(file);
                    }
                }
            }
            dictionary = dictionary.Where(x => x.Files.Count > 0).ToList();
            return dictionary;
        }


        private static Tuple<int, int> GetInfo(ScannedFileInfo file) {
            Match season = new Regex("[s][0-9][0-9]", RegexOptions.IgnoreCase).Match(file.origFile);
            Match episode = new Regex("[e][0-9][0-9]", RegexOptions.IgnoreCase).Match(file.origFile);
            Match special = new Regex("[0-9][0-9][x][0-9][0-9]", RegexOptions.IgnoreCase).Match(file.origFile);
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

        private static List<ScannedFileInfo> FilterSeries(Series series, List<ScannedFileInfo> files) {
            List<ScannedFileInfo> newFiles = new List<ScannedFileInfo>();
            foreach (var file in files) {
                if (CheckAliases(file, series) || CheckAliasesParentDir(file, series)) {
                    file.series = series;
                    newFiles.Add(file);
                }

            }            
            return newFiles;
        }

        private static bool CheckAliasesParentDir(ScannedFileInfo path, Series series) {
            string pathDir = Path.GetFileName(Path.GetDirectoryName(path.origFile.ToUpper()));
            foreach (string alias in series.aliases) {
                string temp = alias.ToUpper();
                if ((pathDir.StartsWith(temp) || 
                    (pathDir.Contains("SEASON") && Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path.origFile.ToUpper()))).StartsWith(temp))) 
                    && IsMatchToIdentifiers(path.origFile)) {
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

        private static bool CheckAliases(ScannedFileInfo file, Series series) {
            foreach (string alias in series.aliases) {
                if (Path.GetFileName(file.origFile.ToUpper()).StartsWith(alias.ToUpper())) {
                    return true;
                }
            }
            return false;
        }

        private static ScannedFile.FileType GetFileType(string file) {
            string[] subExtension = new string[] { ".srt", ".sub" };
            string[] videoExtensions = new string[] { ".mkv", ".avi", ".mp4", ".m4v", ".mov", ".wmv", ".flv" };
            var ext = Path.GetExtension(file);
            if (videoExtensions.Any(x => x == ext)) {
                return ScannedFile.FileType.Video;
            } else if (subExtension.Any(x => x == ext)) {
                return ScannedFile.FileType.Subtitles;
            }
            return ScannedFile.FileType.Video;
        }

        private static List<ScannedFileInfo> FilterExtensions(List<ScannedFileInfo> list) {
            string[] subExtension = new string[] { ".srt", ".sub" };
            string[] videoExtensions = new string[] { ".mkv", ".avi", ".mp4", ".m4v", ".mov", ".wmv", ".flv" };
            List<ScannedFileInfo> filtered = new List<ScannedFileInfo>();
            foreach (var file in list) {
                var ext = Path.GetExtension(file.origFile);
                if (videoExtensions.Any(x => x == ext)) {
                    file.type = ScannedFile.FileType.Video;
                    file.extension = ext;
                    filtered.Add(file);
                } else if (subExtension.Any(x => x == ext)) {
                    file.type = ScannedFile.FileType.Subtitles;
                    file.extension = ext;
                    filtered.Add(file);
                }
            }
            return filtered;
        }

        private static List<ScannedFileInfo> GetFilesInLibrary(Series series) {
            if (series.libraryPath == null) {
                CreateDirectoryForSeries(series);
                return new List<ScannedFileInfo>();
            } else {
                List<ScannedFileInfo> list = new List<ScannedFileInfo>();
                foreach (var file in Directory.GetFiles(series.libraryPath, "*", SearchOption.AllDirectories)) {
                    ScannedFileInfo sfi = new ScannedFileInfo();
                    sfi.origFile = file;
                    sfi.fromLibrary = true;
                    list.Add(sfi);
                }
                return FilterExtensions(list);
            }

        }

        private static List<ScannedFileInfo> GetFilesInDirectory(string path) {
            List<ScannedFileInfo> files = new List<ScannedFileInfo>();
            List<string> paths = new List<string>();
            paths.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories));
            foreach (var file in paths) {
                ScannedFileInfo sfi = new ScannedFileInfo();
                sfi.origFile = file;
                files.Add(sfi);
            }
            return FilterExtensions(files);

        }

        private static List<ScannedFileInfo> GetFilesInScanDirs() {
            List<string> paths = new List<string>();
            List<ScannedFileInfo> files = new List<ScannedFileInfo>();
            if (Directory.Exists(Settings.FirstScanLocation)) paths.AddRange(Directory.GetFiles(Settings.FirstScanLocation, "*", SearchOption.AllDirectories));
            if (Directory.Exists(Settings.SecondScanLocation)) paths.AddRange(Directory.GetFiles(Settings.SecondScanLocation, "*", SearchOption.AllDirectories));
            if (Directory.Exists(Settings.ThirdScanLocation)) paths.AddRange(Directory.GetFiles(Settings.ThirdScanLocation, "*", SearchOption.AllDirectories));
            foreach (var path in paths) {
                ScannedFileInfo sfi = new ScannedFileInfo();
                sfi.origFile = path;
                files.Add(sfi);
            }
            return FilterExtensions(files);
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

        private class ScannedFileInfo{
            public bool fromLibrary = false;
            public string origFile;
            public string newFile;
            public string extension;
            public Series series;
            public Episode episode;
            public Episode.ScannedFile.FileType type;
        }

    }
}
