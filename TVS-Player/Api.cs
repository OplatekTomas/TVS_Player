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
                Properties.Settings.Default.Save();
            }
        }

        //info o epizodě
        public static string apiGet(int season, int episode,int id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/"+id+"/episodes/query?airedSeason="+season+"airedEpisode="+episode);
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/search/series?name="+showname.Replace(" ","."));
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
                MessageBox.Show("ERROR! Are ya sure that data you entered is legit and that you are connected to the internet?", "Error");
                return "error";
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
        public static void apiGetPoster(int id,string showName) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id + "/images/query?keyType=poster");
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\TVS-Player\\"+showName+ "\\Pictures" + "\\Poster\\"+showName+".jpg";
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject parse = JObject.Parse(sr.ReadToEnd());              
                    string url = "http://thetvdb.com/banners/" + parse["data"][0]["fileName"];
                    if (!File.Exists(Path.GetDirectoryName(path))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    using (WebClient client = new WebClient())
                        client.DownloadFile(new Uri(url),path);
                    
                }
            } catch (WebException) {
                MessageBox.Show("Something");
            }
        }


    }
}

