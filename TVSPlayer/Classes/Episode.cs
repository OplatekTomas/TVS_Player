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
        public int airedEpisodeNumber { get; set; }
        public int airedSeason { get; set; }
        public string director { get; set; }
        public List<string> directors { get; set; }
        public string episodeName { get; set; }
        public string firstAired { get; set; }
        public List<string> guestStars { get; set; }
        public int id { get; set; }
        public string imdbId { get; set; }
        public string overview { get; set; }
        public int siteRating { get; set; }
        public int siteRatingCount { get; set; }
        public List<string> writers { get; set; }
        public string filename { get; set; }       
        public static int requestCount = 0;
        public bool hasImage { get; set; }

        /// <summary>
        /// Returns preview image of episode. 
        /// If it doesn't exist returns null
        /// </summary>
        public BitmapImage getImage() {
            if (hasImage) { 
            return new BitmapImage();
            }
            return null;
        }
        /// <summary>
        /// Returns string in format S+airedSeason+E+eiredEpisodeNumber
        /// </summary>
        /// <returns></returns>
        public string getNaming() {
            if (airedSeason >= 10) {
                if (airedEpisodeNumber >= 10) {
                    return "S" + airedSeason + "E" + airedEpisodeNumber;
                } else {
                    return "S" + airedSeason + "E0" + airedEpisodeNumber;
                }
            } else {
                if (airedEpisodeNumber >= 10) {
                    return "S0" + airedSeason + "E" + airedEpisodeNumber;
                } else {
                    return "S0" + airedSeason + "E0" + airedEpisodeNumber;
                }
            }
        }

        /// <summary>
        /// Returns as much details as possible about all Episodes
        /// Please run in a new Thread or as Task. Will take a while
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        public static List<Episode> getAllEPDetailed(TVShow show) {
            List<Episode> list = getAllEP(show);
            int count = 0;
            foreach (Episode e in list) {
                Action a = () => e.getInfo();
                Task t = new Task(a.Invoke);
                t.ContinueWith((t2) => {
                    count++;
                });
                t.Start();
            }
            while (count != list.Count) { Thread.Sleep(50); }
            return list;
        }


        /// <summary>
        /// Fills current instance of episode with detailed info about episode
        /// </summary>
        public bool getInfo() {
            HttpWebRequest request = GeneralAPI.getRequest("https://api.thetvdb.com/episodes/" + id);
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    Episode e = jo["data"].ToObject<Episode>();
                    Copy(e);
                    return true;
                }
            } catch (WebException e) {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Copies all information from episode in parameter to current instance of episode
        /// </summary>
        private void Copy(Episode e) {
            airedEpisodeNumber = e.airedEpisodeNumber;
            airedSeason = e.airedSeason;
            director = e.director;
            directors = e.directors;
            episodeName = e.episodeName;
            firstAired = e.firstAired;
            guestStars = e.guestStars;
            id = e.id;
            imdbId = e.imdbId;
            overview = e.overview;
            siteRating = e.siteRating;
            siteRatingCount = e.siteRatingCount;
            writers = e.writers;
            filename = e.filename;
            
        }

        /// <summary>
        /// Returns all episodes from TVShow with only basic information
        /// Recommended to run in a new thread
        /// </summary>
        public static List<Episode> getAllEP(TVShow show) {
            List<Episode> list = new List<Episode>();
            int id = show.id;
            int page = 1;          
            bool completed = false;
            while (!completed) {
                HttpWebRequest request = GeneralAPI.getRequest("https://api.thetvdb.com/series/" + id + "/episodes?page=" + page);
                try {
                    var response = request.GetResponse();
                    using (var sr = new StreamReader(response.GetResponseStream())) {
                        JObject jo = JObject.Parse(sr.ReadToEnd());
                        foreach (JToken jt in jo["data"]) {
                            Episode s = jt.ToObject<Episode>();
                            list.Add(s);
                        }
                        page++;
                    }
                } catch (WebException e) {
                    //MessageBox.Show(e.Message);
                    completed = true;
                    
                }
            }
            return list;
        }

    }
}
