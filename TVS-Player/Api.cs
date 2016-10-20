using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TVS_Player {
    public static class Api {
        public static string getToken() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/login");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            try {
                using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                    string data = "{\"apikey\": \"0E73922C4887576A\",\"username\": \"Kaharonus\",\"userkey\": \"28E2687478CA3B16\"}";
                    streamWriter.Write(data);
                }
            } catch (WebException) { return null; }
            string text;
            var response = request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream())) {
                text = sr.ReadToEnd();
                text = text.Remove(text.IndexOf("\"token\""), "\"token\"".Length);
                text = text.Split('\"', '\"')[1];
                return text;
            }
        }

        //jméno specifické epizody ( - S01E01 - Space Pilot 3000 )
        public static string apiGet(int season, int episode,string id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/"+id+"/episodes/query?airedSeason="+season+"airedEpisode="+episode);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            string name;
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject parsed = JObject.Parse(sr.ReadToEnd());
                    name = parsed["data"][0]["episodeName"].ToString();                    
                }
            } catch (WebException) {
                MessageBox.Show("ERROR! Please check that you are connected to the internet", "Error!");
                return "error-api";
                }

            if (season < 10) {
                if (episode < 10) {
                    return " - S0" + season + "E0" + episode + name;
                }
                if (episode >= 10) {
                    return " - S0" + season + "E" + episode + name;
                }
            }
             if (season>=10) {
                if (episode < 10) {
                    return " - S" + season + "E0" + episode + name;
                }if (episode >= 10) {
                    return " - S" + season + "E" + episode + name;
                }
            }
            return null;
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
        //Return info about specific show
        public static void apiGetPicture(int id) {
            string token = Properties.Settings.Default.token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/series/" + id);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + token);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject parse = JObject.Parse(sr.ReadToEnd());

                }
            } catch (WebException) {
            }
        }

    }
}

