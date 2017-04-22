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
        public int TVDbID { get; set; }
        public int Id { get; set; }
        public string IMDbId { get; set; }
        public string Name { get; set; }
        public float Rating { get; set; }
        public string Poster { get; set; }
        public string Network { get; set; }
        public string NetworkCountry { get; set; }
        public string Overview { get; set; }
        public string ReleaseDate { get; set; }
        public string AirTime { get; set; }
        public List<string> Genres = new List<string>();
        public List<string> PosterLinks = new List<string>();
        public List<string> AirDays = new List<string>();


        //Searches for a TV Show and returns list of TV Shows
        public static List<TVShow> Search(string name, bool local = false) {
            if (!local) {
                return searchApi(name);
            } return null;
            
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
                if (jt["show"]["externals"]["thetvdb"].ToString() != "") {
                    list.Add(ParseTVMaze(jt["show"]));
                }
            }
            return list;
        }
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
        public List<string> GetAliases() {
            List<string> aliases = new List<string>();
            Regex reg = new Regex(@"\([0-9]{4}\)");
            Regex reg2 = new Regex(@"\.");
            aliases.Add(Name);
            string temp = Name;
            Match m = reg2.Match(temp);
            while (m.Success) {
                temp = temp.Remove(m.Index, 1);
                aliases.Add(Name.Remove(m.Index, 1));
                m = reg2.Match(temp);
            }
            Match snMatch = reg.Match(Name);
            if (snMatch.Success) {
                aliases.Add(reg.Replace(Name, ""));
            }
            foreach (string alias in getAliasToken(TVDbID)) {
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
        private static JToken getAliasToken(int id) {
            HttpWebRequest request = GeneralAPI.getRequest("https://api.thetvdb.com/series/" + id);
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

        private static TVShow ParseTVMaze(JToken value) {
            TVShow s = new TVShow();
            s.TVDbID = Int32.Parse(value["externals"]["thetvdb"].ToString());
            s.Id = Int32.Parse(value["id"].ToString());
            s.Name = value["name"].ToString();
            if (value["rating"]["average"].ToString() != "") {
                s.Rating = float.Parse(value["rating"]["average"].ToString());
            } else {
                s.Rating = 0;
            }
            s.IMDbId = "www.imdb.com/title/" + value["externals"]["imdb"].ToString() + "/?ref_=nv_sr_1";
            if (value["image"].ToString() != "") {
                s.PosterLinks.Add(value["image"]["original"].ToString());
            }
            if (value["network"].ToString() != "") {
                s.Network = value["network"]["name"].ToString();
            }
            if (value["network"]["country"]["name"].ToString() != "") {
                s.NetworkCountry = value["network"]["country"]["name"].ToString();
            }
            if (value["summary"].ToString() != "") {
                s.Overview = RemoveStyle(value["summary"].ToString());
            }
            foreach (JToken genres in value["genres"]) {
                s.Genres.Add(genres.ToString());
            }
            try {
                s.ReleaseDate = DateTime.ParseExact(value["premiered"].ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString("dd.MM.yyyy");
            } catch (Exception) {
                s.ReleaseDate = "";
            }
            if (value["schedule"]["time"].ToString() != "") {
                s.AirTime = value["schedule"]["time"].ToString();
            }
            foreach (JToken day in value["schedule"]["days"]) {
                s.AirDays.Add(day.ToString());
            }
            return s;
        }

    }
}
