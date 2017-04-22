using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TVSPlayer {
    public class TVShow {
        public class Schedule {
            public string time { get; set; }
            public List<object> days { get; set; }
        }

        public class Rating {
            public double? average { get; set; }
        }

        public class Country {
            public string name { get; set; }
            public string code { get; set; }
            public string timezone { get; set; }
        }

        public class Network {
            public int id { get; set; }
            public string name { get; set; }
            public Country country { get; set; }
        }

        public class WebChannel {
            public int id { get; set; }
            public string name { get; set; }
            public Country country { get; set; }
        }

        public class Externals {
            public int? thetvdb { get; set; }
            public string imdb { get; set; }
        }

        public class Image {
            public string original { get; set; }
        }

        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string language { get; set; }
        public List<object> genres { get; set; }
        public string status { get; set; }
        public int runtime { get; set; }
        public string premiered { get; set; }
        public Schedule schedule { get; set; }
        public Rating rating { get; set; }
        public Network network { get; set; }
        public WebChannel webChannel { get; set; }
        public Externals externals { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public List<string> alias = new List<string>();
        public List<string> posters = new List<string>();
        public string poster { get; set; }


        //Searches for a TV Show and returns list of TV Shows
        public static List<TVShow> Search(string name, bool local = false) {
            if (!local) {
                return searchApi(name);
            }
            return null;

        }

        //Searches TVMaze API for TV Shows and returns list of TV Shows
        private static List<TVShow> searchApi(string name) {
            List<TVShow> list = new List<TVShow>();
            name = name.Replace(" ", "+");
            WebRequest wr = WebRequest.Create("http://api.tvmaze.com/search/shows?q=" + name);
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)wr.GetResponse();
            } catch (Exception) {
                return list;
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JArray test = JArray.Parse(responseFromServer);
            foreach (JToken jt in (JToken)test) {
                TVShow s = new TVShow();
                s = jt["show"].ToObject<TVShow>();
                s.summary = RemoveStyle(s.summary);
                list.Add(s);
            }
            return list;
        }

        //Function that removes style from overview text
        private static string RemoveStyle(string line) {
            string[] separator = { "</?p>", "</?em>", "</?strong>", "</?b>", "</?i>", "</?span>" };
            string text = line;
            for (int i = 0; i < separator.Length; i++) {
                Regex reg = new Regex(separator[i]);
                Match m = reg.Match(text);
                while (m.Success) {
                    m = reg.Match(text);
                    if (m.Success) {
                        text = text.Remove(m.Index, m.Length);
                    }
                }
            }
            return text;
        }

        //Gets liases from TVDb API for a TV show
        public List<string> GetAliases() {
            List<string> aliases = new List<string>();
            Regex reg = new Regex(@"\([0-9]{4}\)");
            Regex reg2 = new Regex(@"\.");
            aliases.Add(name);
            string temp = name;
            Match m = reg2.Match(temp);
            while (m.Success) {
                temp = temp.Remove(m.Index, 1);
                aliases.Add(name.Remove(m.Index, 1));
                m = reg2.Match(temp);
            }
            Match snMatch = reg.Match(name);
            if (snMatch.Success) {
                aliases.Add(reg.Replace(name, ""));
            }
            foreach (string alias in getAliasToken()) {
                aliases.Add(alias);
                Match regMatch = reg.Match(alias);
                if (regMatch.Success) {
                    aliases.Add(reg.Replace(alias, ""));
                }
            }
            for (int i = 0; i < aliases.Count(); i++) {
                if (aliases[i].Contains(" ")) {
                    aliases.Add(aliases[i].Replace(" ", "."));
                }
            }
            return aliases;
        }

        // Returns JToken that contains info about aliases
        private JToken getAliasToken() {
            HttpWebRequest request = GeneralAPI.getRequest("https://api.thetvdb.com/series/" + externals.imdb);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    return jo["data"]["aliases"];
                }
            } catch (WebException) {
                return null;
            }
        }

    }
}
