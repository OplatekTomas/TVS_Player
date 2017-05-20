using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TVSPlayer {
    public class TVShow {
        public string added { get; set; }
        public string airsDayOfWeek { get; set; }
        public string airsTime { get; set; }
        public IList<string> aliases { get; set; }
        public string firstAired { get; set; }
        public IList<string> genre { get; set; }
        public int id { get; set; }
        public string imdbId { get; set; }
        public int lastUpdated { get; set; }
        public string network { get; set; }
        public string networkId { get; set; }
        public string overview { get; set; }
        public string rating { get; set; }
        public string runtime { get; set; }
        public string seriesName { get; set; }
        public int siteRating { get; set; }
        public int siteRatingCount { get; set; }
        public string status { get; set; }
        public List<string> posters = new List<string>();
        public string poster { get; set; }
        public int tvmazeId { get; set; }
        private string pathToPoster { get; set; }

        /// <summary>
        /// Searches for a TV Show and returns list of possible TV Shows
        /// </summary>
        /// <param name="name">String to search for</param>
        /// <param name="local">Default is set to false. If set to true wont search API but local DB</param>
        /// <returns>List of TVShows</returns>
        public static List<TVShow> Search(string name, bool local = false) {
            if (!local) {
                return searchApi(name);
            }
            return null;

        }

        /// <summary>
        /// Searches for a TV Show and returns most probable result
        /// </summary>
        /// <param name="name">String to search for</param>
        /// <param name="local">Default is set to false. If set to true wont search API but local DB</param>
        /// <returns>Closest TVShow</returns>
        public static TVShow SearchSingle(string name, bool local = false) {
            if (!local) {
                return searchApiSingle(name);
            }
            return null;

        }

        /// <summary>
        /// Request TV Show from TVMaze API - API returns show with highest probability of a match
        /// </summary>
        /// <param name="name">>Almost anything. Searches api for this</param>
        /// <returns>Closest result</returns>
        private static TVShow searchApiSingle(string name) {
            TVShow s = new TVShow();
            name = name.Replace(" ", "+");
            WebRequest wr = WebRequest.Create("http://api.tvmaze.com/singlesearch/shows?q=" + name);
            wr.Timeout = 2000;
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)wr.GetResponse();
            } catch (Exception e) {
                return s;
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JObject jt = JObject.Parse(responseFromServer);
            if (jt["externals"]["thetvdb"].ToString() != "") {
                s.seriesName = jt["name"].ToString();
                s.id = Int32.Parse(jt["externals"]["thetvdb"].ToString());
                s.tvmazeId = Int32.Parse(jt["id"].ToString());
            }
            return s;
        }

        /// <summary>
        /// Searches TVMaze API for TV Shows and returns list of TV Shows with 
        /// </summary>
        /// <param name="name">Almost anything. Searches api for this</param>
        /// <returns>List of results</returns>
        private static List<TVShow> searchApi(string name) {
            List<TVShow> list = new List<TVShow>();
            name = name.Replace(" ", "+");
            WebRequest wr = WebRequest.Create("http://api.tvmaze.com/search/shows?q=" + name);
            wr.Timeout = 2000;
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)wr.GetResponse();
            } catch (WebException e) {
                MessageBox.Show(e.Message + "\nIf request has timed out it PROBABLY means that you exceeded limit TVMaze servers can take. Please wait around 10 seconds and try again.\n We are sorry for the inconvenience");
                return list;
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JArray test = JArray.Parse(responseFromServer);
            foreach (JToken jt in (JToken)test) {
                if (jt["show"]["externals"]["thetvdb"].ToString() != "") { 
                    TVShow s = new TVShow();
                    s.seriesName = jt["show"]["name"].ToString();
                    s.id = Int32.Parse(jt["show"]["externals"]["thetvdb"].ToString());
                    s.tvmazeId = Int32.Parse(jt["show"]["id"].ToString());
                    list.Add(s);
                }
            }
            return list;
        }

        /// <summary>
        /// Function that removes style from overview text from TVMaze
        /// </summary>
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

        /// <summary>
        /// Returns aliases from TVDb API and adds some custom ones.
        /// </summary>
        public List<string> GetAliases(IList<string> DBAliases) {
            List<string> aliases = new List<string>();
            Regex reg = new Regex(@"\([0-9]{4}\)");
            Regex reg2 = new Regex(@"\.");
            aliases.Add(seriesName);
            string temp = seriesName;
            Match m = reg2.Match(temp);
            while (m.Success) {
                temp = temp.Remove(m.Index, 1);
                aliases.Add(seriesName.Remove(m.Index, 1));
                m = reg2.Match(temp);
            }
            Match snMatch = reg.Match(seriesName);
            if (snMatch.Success) {
                aliases.Add(reg.Replace(seriesName, ""));
            }
            if (DBAliases != null) { 
                foreach (string alias in DBAliases) {
                    aliases.Add(alias);
                    Match regMatch = reg.Match(alias);
                    if (regMatch.Success) {
                        aliases.Add(reg.Replace(alias, ""));
                    }
                }
            }
            for (int i = 0; i < aliases.Count(); i++) {
                if (aliases[i].Contains(" ")) {
                    aliases.Add(aliases[i].Replace(" ", "."));
                }
            }
            return aliases;
        }

        /// <summary>
        /// Gets info from TVDb
        /// </summary>
        public bool GetInfo() {
            HttpWebRequest request = GeneralAPI.getRequest("https://api.thetvdb.com/series/" + id);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    TVShow s = jo["data"].ToObject<TVShow>();
                    
                    Copy(s);
                    aliases = GetAliases(s.aliases);
                    return true;
                }
            } catch (WebException e) {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets info from TVMaze instead of TVDb. Not all arguments are filled
        /// </summary>
        public bool GetInfoTVMaze() {           
            WebRequest wr = WebRequest.Create("http://api.tvmaze.com/shows/" + tvmazeId);
            wr.Timeout = 2000;
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)wr.GetResponse();
            } catch (Exception e) {
                return false;
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            ParseTVMaze(JObject.Parse(responseFromServer));
            return true;
        }

        /// <summary>
        /// Parses information from TVMaze to TVDb TVShow. Not all parameters are filled
        /// </summary>
        private void ParseTVMaze(JObject value) {         
            if (value["rating"]["average"].ToString() != "") {
                rating = value["rating"]["average"].ToString();
            } else {
                rating = "-";
            }
            imdbId = value["externals"]["imdb"].ToString();
            if (value["image"].ToString() != "") {
                poster = value["image"]["original"].ToString();
            }
            if (value["network"].ToString() != "") {
                network = value["network"]["name"].ToString();
            }
            if (value["summary"].ToString() != "") {
             overview = RemoveStyle(value["summary"].ToString());
            }
            genre = new List<string>();
            foreach (JToken genres in value["genres"]) {
                genre.Add(genres.ToString());
            }
            try {
                firstAired = DateTime.ParseExact(value["premiered"].ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString("dd.MM.yyyy");
            } catch (Exception) {
                firstAired = "";
            }
            if (value["schedule"]["time"].ToString() != "") {
                airsTime = value["schedule"]["time"].ToString();
            }
            if (value["schedule"]["days"].Count() > 0) {
                airsDayOfWeek = value["schedule"]["days"][0].ToString();
            }
            if (value["status"].ToString() != "") {
                status = value["status"].ToString();
            }
            
        }


        /// <summary>
        /// Compares ids
        /// </summary>
        public static bool operator ==(TVShow first, TVShow second) {
            if (object.ReferenceEquals(first, null)) {
                return object.ReferenceEquals(second, null);
            }
            if (object.ReferenceEquals(second , null)) {
                return object.ReferenceEquals(first, null);
            }
            return first.CompareID(second);
        }

        /// <summary>
        /// Compares ids
        /// </summary>
        public static bool operator !=(TVShow first, TVShow second) {
            if (object.ReferenceEquals(first, null)) {
                return !object.ReferenceEquals(second, null);
            }
            if (object.ReferenceEquals(second, null)) {
                return !object.ReferenceEquals(first, null);
            }
            return !first.CompareID(second);
        }

        /// <summary>
        /// compares id returns true/false if they are same/different
        /// </summary>
        public bool CompareID(TVShow show) {
            if (id == show.id) {
                return true;
            } else {
                return false;
            }
        }

        
        /// <summary>
        ///  Function that copies all properties from another tv show to this one
        /// </summary>
        private void Copy(TVShow s) {
            this.added = s.added;
            this.airsDayOfWeek = s.airsDayOfWeek;
            this.airsTime = s.airsTime;
            this.aliases = s.aliases;
            this.firstAired = s.firstAired;
            this.genre = s.genre;
            this.id = s.id;
            this.imdbId = s.imdbId;
            this.lastUpdated = s.lastUpdated;
            this.network = s.network;
            this.networkId = s.networkId;
            this.overview = s.overview;
            this.rating = s.rating;
            this.runtime = s.runtime;
            this.seriesName = s.seriesName;
            this.siteRating = s.siteRating;
            this.siteRatingCount = s.siteRatingCount;
            this.status = s.status;
            this.posters = s.posters;
            this.poster = s.poster;
            this.tvmazeId = this.tvmazeId;
        }

    }
}
