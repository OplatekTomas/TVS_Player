using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TVS_Player {
    class Database {
        public static JObject readDb() {
            JObject jo = JObject.Parse(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json"));
            return jo;
        }
        public static void createDb(string dbLoc) {
            string dbText = "{\n\"libraryLocation\": \"" + dbLoc.Replace("\\","\\\\") + "\",\n\"Shows\": [\n]\n}";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.AppendAllText(path, dbText);
        }
        public static void addShowToDb(string id, string showname) {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            string prev = File.ReadAllText(path);
            try {
                JObject kappa = JObject.Parse(prev);
                string damn = kappa["Shows"][0].ToString();
                prev = prev.Remove(prev.Length - 2);
                prev += ",{\"name\": \"" + showname + "\",\"id\": \"" + id + "\",\"poster\": \"" + id + "\"}]}";
            } catch (ArgumentOutOfRangeException) {
                prev = prev.Remove(prev.Length - 3);
                prev += "{\"name\": \"" + showname + "\",\"id\": \"" + id + "\",\"poster\": \"" + id + "\"}]}";
            }
            var file = File.Open(path, FileMode.Create);
            file.Close();
            File.WriteAllText(path, prev);

    }


    }
}
