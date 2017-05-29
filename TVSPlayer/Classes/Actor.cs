using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TVSPlayer {
    public class Actor {
        public int id { get; set; }
        public string image { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public int sortOrder { get; set; }
        public bool hasImage { get; set; }

        /// <summary>
        /// Returns list of Actors. Sorted by how important they are
        /// </summary>
        /// <param name="show">cmon... you have to give it a TVShow</param>
        /// <returns>sorted List<Actor></returns>
        public static List<Actor> GetActors(TVShow show) {
            List<Actor> la = new List<Actor>();
            HttpWebRequest request = GeneralAPI.getRequest("https://api.thetvdb.com/series/" + show.id +"/actors");
            try {
                var response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream())) {
                    JObject jo = JObject.Parse(sr.ReadToEnd());
                    foreach (JToken jt in jo["data"]) {
                        Actor s = jt.ToObject<Actor>();
                        la.Add(s);
                    }
                }
            } catch (WebException e) {}
            la = la.OrderBy(x => x.sortOrder).ToList();
            return la;
        }
    }
}
