using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TVSPlayer {
    class GeneralAPI {

        //Function that returns valid token for TVDB API
        private static string getToken() {
            string token;
            if (Properties.Settings.Default.TokenTime.AddDays(1) < DateTime.Now) {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.thetvdb.com/login");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                try {
                    using (var streamWriter = new StreamWriter(request.GetRequestStream())) {
                        string data = "{\"apikey\": \"0E73922C4887576A\",\"username\": \"Kaharonus\",\"userkey\": \"28E2687478CA3B16\"}";
                        streamWriter.Write(data);
                    }
                } catch (WebException) { MessageBox.Show("Connection error"); token = null; }
                string text;
                try {
                    var response = request.GetResponse();
                    using (var sr = new StreamReader(response.GetResponseStream())) {
                        text = sr.ReadToEnd();
                        text = text.Remove(text.IndexOf("\"token\""), "\"token\"".Length);
                        text = text.Split('\"', '\"')[1];
                        Properties.Settings.Default.Token = text;
                        Properties.Settings.Default.TokenTime = DateTime.Now;
                        Properties.Settings.Default.Save();
                        token = text;
                    }
                } catch (Exception e) {
                    MessageBox.Show("Connection error!\n" + e.Message);
                    token = null;
                }
            } else {
                token = Properties.Settings.Default.Token;
            }
            return token;
        }

        //Function that returns HTTP Web request for TVDB Api
        public static HttpWebRequest getRequest(string link) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
            request.Method = "GET";
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en");
            request.Headers.Add("Authorization", "Bearer " + getToken());
            return request;
        }
    }
}
