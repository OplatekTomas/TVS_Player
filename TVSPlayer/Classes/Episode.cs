using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TVSPlayer {
    class Episode {
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public int season { get; set; }
        public int number { get; set; }
        public string airdate { get; set; }
        public string airtime { get; set; }
        public int runtime { get; set; }
        public Image image { get; set; }
        public string summary { get; set; }
        public List<ScannedFile> files = new List<ScannedFile>();

        public class Image {
            public string medium { get; set; }
            public string original { get; set; }
            public bool hasImage { get; set; }
            public BitmapImage bmp { get; set; }
        }
        public class ScannedFile {
            public enum FileType {
                Video,
                Subtitle
            }
            public string origPath;
            public string path;
            public FileType type;
        }

        /// <summary>
        /// Returns string in format showName - SseasonEepisode - episodeName 
        /// </summary>
        /// <param name="show"></param>
        /// <returns>Returns showName - SxxExx - episodeName</returns>
        public string GetName(TVShow show) {
            return show.seriesName + " - " + GetName() + " - " + name; 
        }

        /// <summary>
        /// Returns string in format S+season+E+eiredEpisodeNumber
        /// </summary>
        /// <returns>Returns SxxExx</returns>
        public string GetName() {
            if (season >= 10) {
                if (number >= 10) {
                    return "S" + season + "E" + number;
                } else {
                    return "S" + season + "E0" + number;
                }
            } else {
                if (number >= 10) {
                    return "S0" + season + "E" + number;
                } else {
                    return "S0" + season + "E0" + number;
                }
            }
        }


        public static void CreateDatabase(List<Episode> list, TVShow show) {
            Database.SaveEpisodes(show, list);
        }

        /// <summary>
        /// Fills current instance of episode with detailed info about episode
        /// </summary>
        public bool getInfo() {
                TVShow s = new TVShow();
                name = name.Replace(" ", "+");
                WebRequest wr = WebRequest.Create("http://api.tvmaze.com/singlesearch/shows?q=" + name);
                wr.Timeout = 2000;
                HttpWebResponse response = null;
                try {
                    response = (HttpWebResponse)wr.GetResponse();
                } catch (Exception x) {
                    return false;
                }
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                JObject jo = JObject.Parse(responseFromServer);
                Episode e = jo.ToObject<Episode>();
                Copy(e);          
            return false;
        }

        /// <summary>
        /// Copies all information from episode in parameter to current instance of episode
        /// </summary>
        private void Copy(Episode e) {
            id = e.id;
            url = e.url;
            name = e.name;
            season = e.season;
            number = e.number;
            airdate = e.airdate;
            airtime = e.airtime;
            runtime = e.runtime;
            image = e.image;
            summary = e.summary;
        }

        /// <summary>
        /// Returns all episodes from TVShow with only basic information
        /// Recommended to run in a new thread
        /// </summary>
        public static List<Episode> getAllEP(TVShow show) {
            List<Episode> list = new List<Episode>();
            WebRequest wr = WebRequest.Create("http://api.tvmaze.com/shows/"+show.tvmazeId+"/episodes");
            wr.Timeout = 2000;
            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)wr.GetResponse();
            } catch (Exception x) {
                return list;
            }
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            JArray jo = JArray.Parse(responseFromServer);
            foreach (JToken jt in jo) {
                Episode e = new Episode();
                e = jt.ToObject<Episode>();
                list.Add(e);
            }

            return list;
        }

    }
}
