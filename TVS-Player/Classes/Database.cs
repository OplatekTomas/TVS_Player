using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TVS_Player {
    public static class DatabaseAPI {
        public static Database database = new Database();

        public static void readDb() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            try {
                string json = File.ReadAllText(path);
                database = JsonConvert.DeserializeObject<Database>(json);
            } catch {
                database = new Database();
            }
        }
        public static void resetDb(string dbLoc, bool save) {
            database = new Database();
            if (save) {
                saveDB();
            }
        }
        public static void addShowToDb(string id, string showname, bool save) {
            SelectedShows newShow = new SelectedShows(id, showname);
            database.Shows.Add(newShow);
            if (save) {
                saveDB();
            }
        }
        public static void saveDB() {
            string json = JsonConvert.SerializeObject(database);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.WriteAllText(path, json);
        }
        public static SelectedShows FindShowByID(string ID) {
            SelectedShows foundShow = null;
            foreach (SelectedShows ss in database.Shows) {
                if (ss.idSel == ID) {
                    foundShow = ss;
                }
            }
            return foundShow;
        }
    }
    public class Database {
        public string libraryLocation;
        public List<SelectedShows> Shows;

        public Database() {
            libraryLocation = String.Empty;
            Shows = new List<SelectedShows>();
        }
    }
    public class SelectedShows {
        public string idSel;
        public string nameSel;
        public string posterFilename;
        public SelectedShows(string id, string showname, string poster = null) {
            this.idSel = id;
            this.nameSel = showname;
            this.posterFilename = poster;
        }
    }
}
