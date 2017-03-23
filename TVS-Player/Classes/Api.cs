using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TVS_Player {
    public static class Api {

        //SHOWS

        public static List<Show.ActorInfo> apiGetActors(int id) {
            HttpWebRequest request = getRequest("https://api.thetvdb.com/series/" + id + "/actors");
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    return ParseActors(sr.ReadToEnd());
                }
            } catch (WebException) {
                return null;
            }
        }
        private static List<string> GetAliases(JToken jt, string sn) {
            List<string> aliases = new List<string>();
            Regex reg = new Regex(@"\([0-9]{4}\)");
            aliases.Add(sn);
            Match snMatch = reg.Match(sn);
            if (snMatch.Success) {
                aliases.Add(reg.Replace(sn, ""));
            }
            foreach (string alias in jt) {
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

        //List of TVShows fitting string
        public static List<Show> apiGet(string showname) {
            List<Show> sh = new List<Show>();
            string json = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.tvmaze.com/search/shows?q=" + showname);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    json = sr.ReadToEnd();
                }
            } catch (WebException) { }
            if (json != null) {
                JObject jo = JObject.Parse(json);
                foreach (JToken j in (JToken)jo) {
                    sh.Add(parseTVMaze(j));
                }
                return sh;
            }
            return null;
        }

        //info o specifickém seriálu
        public static Show apiGet(int id, Show s) {
            HttpWebRequest request = getRequest("https://api.thetvdb.com/series/" + id);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return ParseTVDb(JObject.Parse(sr.ReadToEnd()),s);
                }
            } catch (WebException) {
                return null;
            }

        }

        // Returns number of seasons

        public static int CountSeasons(int id) {
            HttpWebRequest request = getRequest("https://api.thetvdb.com/series/+"+id+"+/episodes/summary");
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    List < int > i = new List<int>();
                    foreach (JToken jt in jo["data"]["airedSeasons"]) {
                        i.Add(Int32.Parse(jt.ToString()));
                    }
                    return i.Max();
                }
            } catch (WebException) {
                return 0;
            }
        }

        //EPISODES

        //Returns all episodes
        public static List<Episode> apiGetEpisodes(int id, int season = -1) {
            if (season == -1) {
                season = CountSeasons(id);
            }
            List<Episode> e = new List<Episode>();
            for (int i = 1; i <= season; i++) { 
                HttpWebRequest request = getRequest("https://api.thetvdb.com/series/" + id + "/episodes/query?airedSeason=" + i);
                try {
                    var response = request.GetResponse();
                    using (var sr = new StreamReader(response.GetResponseStream())) {
                        JObject jo = JObject.Parse(sr.ReadToEnd());
                        foreach (JToken jt in (JToken)jo) {
                            e.Add(ParseEP(jt));
                        }
                    }
                } catch (WebException) {
                    return null;
                }
            }
            return e;
        }
        public static Episode apiGet(int season, int episode, int id) {
            HttpWebRequest request = getRequest("https://api.thetvdb.com/series/" + id + "/episodes/query?airedSeason=" + season + "airedEpisode=" + episode);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    return ParseEP(jo);
                }
            } catch (WebException) {
                return null;
            }
        }
        public static Episode apiGetEP(int id) {
            HttpWebRequest request = getRequest("https://api.thetvdb.com/episodes/" + id);
            try { 
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    return ParseEPDetail(jo);
                }
            } catch (WebException) {
                return null;
            }
        }


        //OTHERS

        private static List<Show.ActorInfo> ParseActors(JToken jo) {
            List<Show.ActorInfo> actors = new List<Show.ActorInfo>();
            JToken acs = jo["data"];
            foreach (JToken jt in acs) {
                Show.ActorInfo ai = new Show.ActorInfo();
                ai.character = jt["role"].ToString();
                ai.name = jt["name"].ToString();
                ai.link = jt["image"].ToString();
                ai.roleImportance = Int32.Parse(jt["sortOrder"].ToString());
                actors.Add(ai);
            }
            actors = actors.OrderBy(o => o.roleImportance).ToList();
            return actors;
        }

        private static Episode ParseEPDetail(JToken jt) {
            JToken jo = jt["data"];
            Episode e = new Episode();
            e.episode = Int32.Parse(jo["airedEpisodeNumber"].ToString());
            e.id = Int32.Parse(jo["id"].ToString());
            e.name = jo["episodeName"].ToString();
            e.release = DateTime.ParseExact(jo["firstAired"].ToString(), "yyyy-MM-dd", null).ToString("dd.MM.yyyy");
            e.season = Int32.Parse(jo["airedSeason"].ToString());
            e.overview = jo["overview"].ToString();
            e.link = jo["filename"].ToString();          
            return e;
        }

        private static Show parseTVMaze(JToken jo) {
            JToken jt = jo["show"];
            Show s = new Show();
            s.name = jt["name"].ToString();
            foreach (JToken j in jt["genre"]) {
                s.genre.Add(j.ToString());
            }
            s.release = DateTime.ParseExact(jt["release"].ToString(), "yyyy-MM-dd", null).ToString("dd.MM.yyyy");
            s.id.TVMaze = Int32.Parse(jt["id"].ToString());
            s.id.TVDb = Int32.Parse(jt["externals"]["thetvdb"].ToString());
            s.id.IMDb = jt["externals"]["thetvdb"].ToString();
            s.overview = jt["summary"].ToString();
            s.status = jt["status"].ToString();
            s.airday = jt["schedule"]["days"][0].ToString();
            s.airtime = jt["schedule"]["time"].ToString();
            s.rating = jt["rating"]["average"].ToString();
            return s;
        }

        private static Show ParseTVDb(JToken jo, Show s) {
            foreach (JToken j in jo["data"]["aliases"]) {
                s.aliases.Add(j.ToString());
            }
            s.actors = apiGetActors(s.id.TVDb);
            s.bannerLink = jo["data"]["banner"].ToString();
            s.EPlenght = Int32.Parse(jo["data"]["runtime"].ToString());
            return s;
        }

        private static Episode ParseEP(JToken jo) {
            Episode e = new Episode();
            e.episode = Int32.Parse(jo["airedEpisodeNumber"].ToString());
            e.id = Int32.Parse(jo["id"].ToString());
            e.name = jo["episodeName"].ToString();
            e.release = DateTime.ParseExact(jo["firstAired"].ToString(), "yyyy-MM-dd", null).ToString("dd.MM.yyyy");
            e.season = Int32.Parse(jo["airedSeason"].ToString());
            e.overview = jo["overview"].ToString();
            return e;
        }

        //ziska standartni poster s moznou volbou miniatury
        public static bool apiGetPoster(int id, bool isThumbnail) {
            String path = Helpers.path;
            if (!isThumbnail) {
                path += id.ToString() + "\\" + id.ToString() + ".jpg";
            } else {
                if (!Directory.Exists(Helpers.path + id.ToString() + "\\Thumbnails\\")) {
                    Directory.CreateDirectory(Helpers.path + id.ToString() + "\\Thumbnails\\");
                }
                path += id.ToString() + "\\Thumbnails\\" + id.ToString() + ".jpg";
            }
            if (!File.Exists(path)) {
                HttpWebRequest request = getRequest("https://api.thetvdb.com/series/" + id + "/images/query?keyType=poster");

                try {
                    var response = request.GetResponse();
                    using (var sr = new StreamReader(response.GetResponseStream())) {
                        JObject parse = JObject.Parse(sr.ReadToEnd());
                        string url;
                        if (!isThumbnail) {
                            url = "http://thetvdb.com/banners/" + parse["data"][0]["fileName"];
                        } else {
                            url = "http://thetvdb.com/banners/" + parse["data"][0]["thumbnail"];
                        }
                        if (!File.Exists(Path.GetDirectoryName(path))) {
                            Directory.CreateDirectory(Path.GetDirectoryName(path));
                        }
                        using (WebClient client = new WebClient())
                            client.DownloadFile(new Uri(url), path);
                        return true;
                    }
                } catch (WebException) {
                    MessageBox.Show("ERROR! Cannot download poster image or show doesn't have any posters!", "Error");
                    return false;
                }
            }
            return true;
        }
        //ziska urcity poster s volbou miniatury
        public static void apiGetPoster(int id, string filename, int i, bool isThumbnail) {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!isThumbnail) {
                path += "\\TVS-Player\\" + id.ToString() + "\\" + filename;
            } else {
                if (!Directory.Exists(path + "\\TVS-Player\\" + id.ToString() + "\\Thumbnails\\")) {
                    Directory.CreateDirectory(path + "\\TVS-Player\\" + id.ToString() + "\\Thumbnails\\");
                }
                path += "\\TVS-Player\\" + id.ToString() + "\\Thumbnails\\" + filename;
            }
            if (!File.Exists(path)) {
                try {
                    string url;
                    if (!isThumbnail) {
                        url = "http://thetvdb.com/banners/posters/" + filename;
                    } else {
                        url = "http://thetvdb.com/banners/_cache/posters/" + filename;
                    }
                    if (!File.Exists(Path.GetDirectoryName(path))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    using (WebClient client = new WebClient())
                        client.DownloadFile(new Uri(url), path);
                } catch (WebException) {
                    MessageBox.Show("Error while downloading specific show image", "ERROR");
                }
            }
        }
        //ziska seznam vsech posteru
        public static string apiGetAllPosters(int id) {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\TVS-Player\\" + id.ToString() + "\\" + id.ToString() + ".jpg";
            HttpWebRequest request = getRequest("https://api.thetvdb.com/series/" + id + "/images/query?keyType=poster");
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();
                }
            } catch (WebException) {
                MessageBox.Show("Error while getting info about all posters", "ERROR");
                return "FUCK YOU";
            }
        }
        //Gets Token for API access
        public static void getToken() {
            if (Properties.Settings.Default.tokenTime == null || DateTime.Now.Subtract(Properties.Settings.Default.tokenTime).TotalHours >= 23.5f) {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/login");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                try {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                        string data = "{\"apikey\": \"0E73922C4887576A\",\"username\": \"Kaharonus\",\"userkey\": \"28E2687478CA3B16\"}";
                        streamWriter.Write(data);
                    }
                } catch (WebException) { MessageBox.Show("Make sure you are connected to the internet!"); }
                string text;
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    text = sr.ReadToEnd();
                    text = text.Remove(text.IndexOf("\"token\""), "\"token\"".Length);
                    text = text.Split('\"', '\"')[1];
                    Properties.Settings.Default.token = text;
                    Properties.Settings.Default.tokenTime = DateTime.Now;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private static HttpWebRequest getRequest(string link) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + Properties.Settings.Default.token);
            return request;
        }
    }   
}
