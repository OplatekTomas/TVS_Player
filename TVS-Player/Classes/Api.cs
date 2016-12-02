using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TVS_Player {
    public static class Api {
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

        //info o epizodě
        public static string apiGet(int season, int episode, int id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id + "/episodes/query?airedSeason=" + season + "airedEpisode=" + episode);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();
                }
            } catch (WebException) {
                MessageBox.Show("ERROR! Please check that you are connected to the internet", "Error!");
                return "error-api";
            }
        }

        //Info o možných seriálech
        public static string apiGet(string showname) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/search/series?name=" + showname.Replace(" ", "+"));
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();
                }
            } catch (WebException) {
                return null;
            }
        }
        //info o specifickém seriálu
        public static string apiGet(int id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();

                }
            } catch (WebException) {
                MessageBox.Show("ERROR! Are ya sure that you are connected to the internet?", "Error");
                return "error";
            }

        }
        //seznam vsech serii
        public static string apiGetSeasons(int id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id + "/episodes/summary");
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();

                }
            } catch (WebException) {
                MessageBox.Show("ERROR! Are ya sure that you are connected to the internet?", "Error");
                return "error";
            }
        }
        //seznam epizod v serii
        public static string apiGetEpisodesBySeasons(int id, int season) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id + "/episodes/query?airedSeason=" + season);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();

                }
            } catch (WebException) {
                MessageBox.Show("ERROR! Are ya sure that you are connected to the internet?", "Error");
                return "error";
            }
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
            string token = Properties.Settings.Default.token;
            if (!File.Exists(path)) {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id + "/images/query?keyType=poster");
                request.Method = "GET";
                request.Accept = "application/json";
                request.Headers.Add("Accept-Language", "en");
                request.Headers.Add("Authorization", "Bearer " + token);

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
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id + "/images/query?keyType=poster");
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
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
        public static string apiGetActors(int id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id+"/actors");
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    return sr.ReadToEnd();
                }
            } catch (WebException) {
                MessageBox.Show("ERROR! Are ya sure that you are connected to the internet?", "Error");
                return "error";
            }
        }
    }
}
