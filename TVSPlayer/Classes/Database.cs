using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using TVS.API;
using Newtonsoft.Json.Linq;

namespace TVSPlayer {
    class Database {
        static List<string> filesUsed = new List<string>();

        #region Series
        /// <summary>
        /// Returns all series in database
        /// </summary>
        /// <returns>All series in database or null in case of errors</returns>
        public static List<Series> GetAllSeries() {
            return new List<Series>();
        }
        /// <summary>
        /// Searches database for Series by id
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>Series or null in case of errors</returns>
        public static Series GetSeries(int id) {
            return new Series();
        }
        /// <summary>
        /// Adds multiple Series to database
        /// </summary>
        /// <param name="series">Which Series to add</param>
        public static void AddSeries(List<Series> series) {
            List<Series> list = SeriesAdder(series);
            string json = JsonConvert.SerializeObject(list);
            WriteToFile(Helper.data + "Series.tvsp", json);
        }
        /// <summary>
        /// Adds single Series to database
        /// </summary>
        /// <param name="series">Which Series to add</param>
        public static void AddSeries(Series series) {
            List<Series> list = SeriesAdder(new List<Series>() { series });
            string json = JsonConvert.SerializeObject(list);
            WriteToFile(Helper.data + "Series.tvsp", json);
        }

        private static List<Series> SeriesAdder(List<Series> series) {
            string original = ReadFromFile(Helper.data + "Series.tvsp");
            JObject jo = JObject.Parse(original);
            List<Series> list = jo.ToObject<List<Series>>();
            list.AddRange(series);
            return list;
        }

        #endregion

        #region Episode
        /// <summary>
        /// Gets all episodes in Series
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>All Episodes in Series or null in case of errors</returns>
        public static List<Episode> GetAllEPisodes(int id) {
            return new List<Episode>();
        }
        /// <summary>
        /// Searches for multiple Episodes
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="season">Which season to search</param>
        /// <returns>List of Episodes or null in case of errors</returns>
        public static List<Episode> GetEpisode(int id, int season) {
            return new List<Episode>();
        }
        /// <summary>
        /// Searches for Episode
        /// </summary>
        /// <param name="id">TVDb id of Episode</param>
        /// <param name="fromAPI"> If data is not complete also search TVDb API</param>
        /// <returns>Episode or null in case of errors</returns>
        public static Episode GetEpisode(int id, bool fromAPI = false) {
            return new Episode();
        }
        /// <summary>
        /// Searches for Episode
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="season">Which season is Episode from</param>
        /// <param name="episode">Which episode it is</param>
        /// <param name="fromAPI">If data is not complete also search TVDb API default: false</param>
        /// <returns>Episode or null in case of errors</returns>
        public static Episode GetEpisode(int id, int season, int episode, bool fromAPI = false) {
            return new Episode();
        }
        /// <summary>
        /// Adds multiple Episodes to any Series by TVDb id
        /// </summary>
        /// <param name="episodes">What episodes to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddEpisode(List<Episode> episodes, int id) {

        }
        /// <summary>
        /// Adds single Episode to any Series by TVDb id
        /// </summary>
        /// <param name="episode">What episode to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddEpisode(Episode episode, int id) {

        }
        #endregion

        #region Actor
        /// <summary>
        /// Returns all actors in a Series by TVDb id
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>List of Actors or null if noone was found</returns>
        public static List<Actor> GetActors(int id) {
            return new List<Actor>();

        }
        /// <summary>
        /// Gets all roles of an actor by name
        /// </summary>
        /// <param name="name">name of an actor</param>
        /// <returns>List of Actors or null if nothing was found</returns>
        public static List<Actor> GetRoles(string name) {
            return new List<Actor>();
        }
        /// <summary>
        /// Gets Actor by name of a role.
        /// </summary>
        /// <param name="role">string representing name of a character</param>
        /// <returns>Actor or null if noone was found</returns>
        public static Actor GetActor(string role) {
            return new Actor();
        }

        /// <summary>
        /// Adds multiple Actors to any Series by TVDb id
        /// </summary>
        /// <param name="actors">What Actors to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddActor(List<Actor> actors, int id) {

        }
        /// <summary>
        /// Adds single Actor to any Series by TVDb id
        /// </summary>
        /// <param name="actor">What Actor to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddActor(Actor actor, int id) {

        }
        #endregion

        #region Poster
        /// <summary>
        /// Returns all Posters by TVDb id
        /// </summary>
        /// <param name="id">TVDb id of Series </param>
        /// <returns></returns>
        public static List<Poster> GetPosters(int id) {
            return new List<Poster>();
        }
        /// <summary>
        /// Returns currently selected poster as BitmapImage so it can be set as Image.Source
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>BitmapImage with poster in it or default image when no poster is downloaded</returns>
        public static BitmapImage GetSelectedPoster(int id) {
            return new BitmapImage();
        }
        
        /// <summary>
        /// Adds multiple Posters to any Series by TVDb id
        /// </summary>
        /// <param name="posters">What Posters to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddPoster(List<Poster> posters,int id) {

        }
        /// <summary>
        /// Adds single poster to any Series by TVDb id
        /// </summary>
        /// <param name="poster">What Poster to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddPoster(Poster poster, int id) {

        }
        #endregion

        private static string ReadFromFile(string path) {
            int i = 0;
            while (i >= filesUsed.Count) {
                if (filesUsed[i] == path) {
                    i = 0;
                    Thread.Sleep(5);
                } else {
                    i++;
                }
            }
            string text;
            filesUsed.Add(path);          
            using (FileStream fs = File.Open(path,FileMode.OpenOrCreate)) {
                StreamReader sr = new StreamReader(fs);
                text = sr.ReadToEnd();
            }
            filesUsed.Remove(path);
            return text;
        }

        private static string ObjectToJson(object obj) {
             return JsonConvert.SerializeObject(obj);
        }

        private static void WriteToFile(string path, string json) {
            int i = 0;
            while (i >= filesUsed.Count) {
                if (filesUsed[i] == path) {
                    i = 0;
                    Thread.Sleep(5);
                } else {
                    i++;
                }
            }
            filesUsed.Add(path);
            using (FileStream fs = File.Open(path, FileMode.Create)) {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
            }
            filesUsed.Remove(path);
        }

       
    }
}
