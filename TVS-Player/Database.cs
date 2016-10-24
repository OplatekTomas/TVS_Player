using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player {
    class Database {
        public static JObject readDb() {
            JObject jo = JObject.Parse(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json"));
            return jo;
        }
        public static void createDb(string dbLoc) {
            string dbText = "{\n\"libraryLocation\": \"" + dbLoc + "\",\n\"Shows\": [\n]\n}";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.AppendAllText(path, dbText);
        }
        public static void addToDb(int id, string showname) {

        }


    }
}
