using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace TVS_Player {

    public static class AppSettings {
        public static void SaveDB(SettingsDB settings) {
            string tempJS = JsonConvert.SerializeObject(settings);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\Settings.TVSP";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, tempJS);
        }
        public static SettingsDB ReadDB() {
            string path = Helpers.path + "Settings.TVSP";
            JObject jo = new JObject();
            try {
                string json = File.ReadAllText(path);
                jo = JObject.Parse(json);
            } catch {
                return new SettingsDB(null);
            }
            return jo.ToObject<SettingsDB>();
        }
        public static string GetLibLocation() {
            SettingsDB sdb = ReadDB();
            return sdb.LibLocation;
        }
        public static void SetLibLocation(string path) {
            SettingsDB sdb = ReadDB();
            sdb.LibLocation = path;
            SaveDB(sdb);
        }
        public static void SetBuildInPlayer(bool use) {
            SettingsDB sdb = ReadDB();
            sdb.BuildInPlayer = use;
            SaveDB(sdb);
        }
        public static bool GetBuildInPlayer() {
            SettingsDB sdb = ReadDB();
            return sdb.BuildInPlayer;
        }
        public static void AddLocation(string path) {
            SettingsDB sdb = ReadDB();
            sdb.ScanPaths.Add(path);
            SaveDB(sdb);
        }
        public static List<string> GetLocations() {
            SettingsDB sdb = ReadDB();
            return sdb.ScanPaths;
        }
        public static void RemoveLocation(string path) {
            SettingsDB sdb = ReadDB();
            sdb.ScanPaths.Remove(path);
            SaveDB(sdb);
        }
        public static void EditLocation(string originalpath,string newpath) {
            SettingsDB sdb = ReadDB();
            sdb.ScanPaths[sdb.ScanPaths.IndexOf(originalpath)] = newpath;
            SaveDB(sdb);
        }
    }

    public static class DatabaseShows {
        public static List<Show> ReadDb() {
            List<Show> ss = new List<Show>();
            string path = Helpers.path + "Shows.TVSP";
            JArray jo = new JArray();
            try {
                string json = File.ReadAllText(path);
                jo = JArray.Parse(json);
            } catch {
            }
            foreach (JToken jt in jo) {
                ss.Add(jt.ToObject<Show>());
            }
            return ss;
        }

        public static bool CheckIfExists(int id) {
            foreach (Show ss in ReadDb()) {
                if (ss.id == id) {
                    return true;
                }
            }
            return false;
        }

        public static void AddShowToDb(int id, string showname) {
            List<Show> ss = ReadDb();
            Show newShow = new Show(id, showname);
            if (!CheckIfExists(id)) {
                ss.Add(newShow);
            } else {
                DialogResult dialogResult = MessageBox.Show("TV Show is already in database do you want to rewrite it?", "TV Show already exists", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) {
                    RemoveShowFromDb(id);
                    AddShowToDb(id,showname);
                }
            }
            SaveDB(ss);
        }
        public static void AddShowToDb(Show s) {
            List<Show> ss = ReadDb();
            Show newShow = new Show(s.id, s.name);
            if (!CheckIfExists(s.id)) {
                ss.Add(newShow);
            } else {
                DialogResult dialogResult = MessageBox.Show("TV Show is already in database do you want to rewrite it?", "TV Show already exists", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) {
                    RemoveShowFromDb(s.id);
                    AddShowToDb(s);
                }
            }
            SaveDB(ss);
        }
        public static void RemoveShowFromDb(int id) {
            List<Show> ss = ReadDb();
            Show sel = ss.Find(s=> s.id == id);
            ss.Remove(sel);
            SaveDB(ss);
        }
        public static void SaveDB(List<Show> ss) {
            string tempJS = JsonConvert.SerializeObject(ss);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\Shows.TVSP";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, tempJS);
        }
        public static Show FindShow(int ID) {
            Show foundShow = null;
            foreach (Show ss in ReadDb()) {
                if (ss.id == ID) {
                    foundShow = ss;
                }
            }
            return foundShow;
        }
        public static int GetSeasons(int id) {
            return DatabaseEpisodes.ReadDb(id).Max(e => e.season);
        }
        public static void Edit(Show s) {
            List<Show> ls = ReadDb();
            int index = ls.FindIndex(sh => sh.id == s.id);
            ls[index] = s;
            SaveDB(ls);
        }
    }
    public class DatabaseEpisodes {
        public static void CreateDB(int id, List<Episode> l) {
            string path = Helpers.path + id + "\\Episodes.TVSP";
            string json = JsonConvert.SerializeObject(l);
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, json);
        }
        public static void AddEPToDb(int id,Episode e) {
            List <Episode> list = ReadDb(id);
            list.Add(e);
            CreateDB(id, list);           
        }
        public static int GetEpPerSeason(int id, int season) {
            List<Episode> list = ReadDb(id);
            int count = 0;
            foreach (Episode e in list) {
                if (e.season == season) {
                    count++;
                }
            }
            return count;
        }

        public static string GetSeasonRelease(int id, int season) {
            List<Episode> list = ReadDb(id);
            return list.FindAll(s => s.season == season).Min(e => e.release);
        }

        public static List<Episode> ReadDb(int id) {
            List<Episode> e = new List<Episode>();
            string path = Helpers.path + id + "\\Episodes.TVSP";
            JArray jo = new JArray();
            try {
                string json = File.ReadAllText(path);
                jo = JArray.Parse(json);
            } catch {
            }
            foreach (JToken jt in jo) {
                e.Add(jt.ToObject<Episode>());             
            }
            return e;
        }

    }

    public class SearchIndexDB {
        public static void SaveDB(List<SearchItem> list) {
            string tempJS = JsonConvert.SerializeObject(list);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\SearchIndex.TVSP";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, tempJS);
        }
        public static List<SearchItem> ReadDB(){
            List<SearchItem> e = new List<SearchItem>();
            string path = Helpers.path + "SearchIndex.TVSP";
            JArray jo = new JArray();
            try {
                string json = File.ReadAllText(path);
                jo = JArray.Parse(json);
            } catch {
            }
            foreach (JToken jt in jo) {
                e.Add(jt.ToObject<SearchItem>());
            }
            return e;
        }
    }
    public class SearchItem {
        public Episode episodeObject;
        public Show show;
        public string episodeNumber;
        public string seasonNumber;
        public SearchItem(Episode episode, Show show) {
            episodeObject = episode;
            this.show = show;
            this.seasonNumber = SE(episode.season);
            this.episodeNumber = EP(episode.episode);
        }
        [JsonConstructor]
        public SearchItem(Episode episode, Show show,string seasonNumber,string episodeNumber) {
            episodeObject = episode;
            this.show = show;
            this.seasonNumber = seasonNumber;
            this.episodeNumber = episodeNumber;
        }

        private string EP(int number) {
            if (number < 10) {
                return "E0" + number;
            } else {
                return "E" + number;
            }
        }
        private string SE(int number) {
            if (number < 10) {
                return "S0" + number;
            } else {
                return "S" + number;
            }
        }
    }
        
        public class Show {
        public int id;
        public string name;
        public string posterFilename;
        public Show(int id, string showname, string poster = null) {
            this.id = id;
            this.name = showname;
            this.posterFilename = poster;
        }
    }
    public class Episode {
        public string name;
        public int season;
        public int episode;
        public int id;
        public string release;
        public bool downloaded;
        public List<string> locations;
        public Episode(string name, int season, int episode, int id, string releaseDate, bool downloaded, List<string> l) {
            this.name = name;
            this.season = season;
            this.episode = episode;
            release = releaseDate;
            this.id = id;
            this.downloaded = downloaded;
            locations = l;
        }
    }
    public class SettingsDB {
        public string LibLocation;
        public bool BuildInPlayer { get; set; }
        public List<string> ScanPaths = new List<string>();

        public SettingsDB(string LibLocation) {
            this.LibLocation = LibLocation;
        }

    }
}
