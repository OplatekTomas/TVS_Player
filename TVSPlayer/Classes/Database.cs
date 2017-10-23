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
using System.Net;

namespace TVSPlayer {
    class Database {
        static string db = Helper.data;

        #region Series
        /// <summary>
        /// Returns all series in database
        /// </summary>
        /// <returns>All series in database</returns>
        public static List<Series> GetSeries() {
            string original = ReadFromFile(Helper.data + "Series.tvsp");
            List<Series> list = new List<Series>();
            if (!String.IsNullOrEmpty(original)) {
                JArray jo = JArray.Parse(original);
                list = jo.ToObject<List<Series>>();
            }
            return list.OrderBy(s => s.seriesName).ToList();
        }
        
        /// <summary>
        /// Searches database for Series by id
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>Series or null in case of errors</returns>
        public static Series GetSeries(int id) {
            List<Series> all = GetSeries();
            try {
                return all.Single(s => s.id == id);
            } catch (Exception) {
                return null;
            }
        }
       
        /// <summary>
        /// Adds multiple Series to database
        /// </summary>
        /// <param name="series">Which Series to add</param>
        public static void AddSeries(List<Series> series) {
            SeriesAdder(series);
        }
       
        /// <summary>
        /// Adds single Series to database
        /// </summary>
        /// <param name="series">Which Series to add</param>
        public static void AddSeries(Series series) {
            SeriesAdder(new List<Series>() { series });
        }        
        
        /// <summary>
        /// Code behind both overloads of AddSeries()
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        private static void SeriesAdder(List<Series> series) {
            string original = ReadFromFile(Helper.data + "Series.tvsp");
            List<Series> list = new List<Series>();
            if (!String.IsNullOrEmpty(original)) {
                JArray jo = JArray.Parse(original);          
                list = jo.ToObject<List<Series>>();
            }
            list.AddRange(series);
            foreach (Series s in series) {
                if(!Directory.Exists(db + s.id)) Directory.CreateDirectory(db + s.id);
            }
            string json = JsonConvert.SerializeObject(list);
            WriteToFile(Helper.data + "Series.tvsp", json);
        }
       
        /// <summary>
        /// Removes Series from database
        /// </summary>
        /// <param name="id">TVDb id of Series to remove</param>
        /// <returns>true if remove was successful or false if it was not</returns>
        public static bool RemoveSeries(int id) {
            List<Series> s = GetSeries();
            try {
                s.Remove(s.Single(se => se.id == id));
                string json = JsonConvert.SerializeObject(s);
                WriteToFile(Helper.data + "Series.tvsp", json);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Edits Series in database
        /// </summary>
        /// <param name="id">TVDb id of Series to edit</param>
        /// <param name="series">New Series that will replace the old one</param>
        /// <returns>true if edit was successful or false if it was not</returns>
        public static bool EditSeries(int id, Series series) {
            List<Series> allSeries = GetSeries();
            if (RemoveSeries(id)) {
                AddSeries(series);
                return true;
            }
            return false;

        }

        #endregion

        #region Episode
        /// <summary>
        /// Gets all episodes in Series
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>All Episodes in Series in database</returns>
        public static List<Episode> GetEpisodes(int id) {
            string orig = ReadFromFile(db + id + "\\Episodes.tvsp");
            List<Episode> eps = new List<Episode>();
            if (!String.IsNullOrEmpty(orig)) {
                JArray jo = JArray.Parse(orig);
                eps = jo.ToObject<List<Episode>>();
            }
            return eps.OrderBy(s => s.airedSeason).ThenBy(s=>s.airedEpisodeNumber).ToList();
        }
        
        /// <summary>
        /// Searches for multiple Episodes
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="season">Which season to search</param>
        /// <returns>List of Episodes</returns>
        public static List<Episode> GetEpisodes(int id, int season) {
            List<Episode> eps = GetEpisodes(id);
            List<Episode> final = new List<Episode>();
            foreach (Episode ep in eps) {
                if (ep.airedSeason == id) {
                    final.Add(ep);
                }
            }
            return final;
        }
       
        /// <summary>
        /// Searches for Episode
        /// </summary>
        /// <param name="id">TVDb id of Episode</param>
        /// <param name="fromAPI"> If data is not complete also search TVDb API</param>
        /// <returns>Episode or null in case of errors</returns>
        public static Episode GetEpisode(int id, bool fromAPI = false) {
            Episode epi = null;
            bool doBreak = false;
            Series series = null;
            foreach (Series s in GetSeries()) {
                foreach (Episode e in GetEpisodes(s.id)) {
                    if (e.id == id) {
                        epi = e;
                        series = s;
                        doBreak = true;
                        break;
                    }
                }
                if (doBreak) break;
            }
            if (epi != null && epi.seriesId == null && fromAPI) {
                epi = Episode.GetEpisode(epi.id);
                EditEpisode(series.id,epi.id,epi);
            }
            return epi;
        }

        /// <summary>
        /// Removes episode from database
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="epId">TVDB id of Episode</param>
        /// <returns>true if remove was successful or false if it was not</returns>
        public static bool RemoveEpisode(int id, int epId) {
            List<Episode> eps = GetEpisodes(id, epId);
            try {
                eps.Remove(eps.Single(se => se.id == epId));
                string json = JsonConvert.SerializeObject(eps);
                WriteToFile(db + id + "\\Episode.tvsp", json);
                return true;
            } catch (Exception) {
                return false;
            }
        }
        
        /// <summary>
        /// Edits episode in database
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="epId">TVDb id of Episode</param>
        /// <param name="ep">new Episode that will replace the old one</param>
        /// <returns>true if edit was successful or false if it was not</returns>
        public static bool EditEpisode(int id, int epId, Episode ep) {
            if (RemoveEpisode(id, epId)) {
                AddEpisode(id, ep);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Searches for Episode
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="epId">TVDb id of Episode</param>
        /// <param name="fromAPI"> If data is not complete also search TVDb API</param>
        /// <returns>Episode or null in case of errors</returns>
        public static Episode GetEpisode(int id, int epId, bool fromAPI = false) {
            List<Episode> le = GetEpisodes(id);
            Episode epi = null;
            foreach (Episode ep in le) {
                if (ep.id == epId) {
                    epi = ep;
                    break;
                }
            }
            if (epi != null && epi.seriesId == null && fromAPI) {
                epi = Episode.GetEpisode(epi.id);
                EditEpisode(id, epi.id, epi);
            }
            return epi;
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
            List<Episode> le = GetEpisodes(id);
            Episode epi = null;
            foreach (Episode ep in le) {
                if (ep.airedSeason == season && ep.airedEpisodeNumber == episode) {
                    epi = ep;
                    break;
                }
            }
            if (epi != null && epi.seriesId == null && fromAPI) {
                epi = Episode.GetEpisode(epi.id);
                EditEpisode(id, epi.id, epi);
            }
            return epi;
        }
       
        /// <summary>
        /// Adds multiple Episodes to any Series by TVDb id
        /// </summary>
        /// <param name="episodes">What episodes to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddEpisode(int id,List<Episode> episodes) {
            EpisodeAdder(id, episodes);
        }
       
        /// <summary>
        /// Adds single Episode to any Series by TVDb id
        /// </summary>
        /// <param name="episode">What episode to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddEpisode(int id, Episode episode) {
            EpisodeAdder(id,new List<Episode>() { episode });
        }
      
        /// <summary>
        /// Code behind both overloads for AddEpisode
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="episode">List of Episodes to be added</param>
        private static void EpisodeAdder(int id,List<Episode> episode) {
            string original = ReadFromFile(db + id + "\\Episodes.tvsp");
            List<Episode> list = new List<Episode>();
            if (!String.IsNullOrEmpty(original)) {
                JArray jo = JArray.Parse(original);
                list = jo.ToObject<List<Episode>>();
            }
            list.AddRange(episode);
            string json = JsonConvert.SerializeObject(list);
            WriteToFile(db + id + "\\Episodes.tvsp", json);
        }

        #endregion

        #region Actor
       
        /// <summary>
        /// Returns all actors in a Series by TVDb id
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>List of Actors or null if noone was found</returns>
        public static List<Actor> GetActors(int id) {
            string orig = ReadFromFile(db + id + "\\Actors.tvsp");
            List<Actor> eps = new List<Actor>();
            if (!String.IsNullOrEmpty(orig)) {
                JArray jo = JArray.Parse(orig);
                eps = jo.ToObject<List<Actor>>();
            }
            return eps.OrderBy(s => s.sortOrder).ToList();

        }
       
        /// <summary>
        /// Gets all roles of an actor by name
        /// </summary>
        /// <param name="name">name of an actor</param>
        /// <returns>List of Actors - empty if nothing was found</returns>
        public static List<Actor> GetRoles(string name) {
            List<Actor> la = new List<Actor>();
            foreach (Series s in GetSeries()) {
                foreach (Actor a in GetActors(s.id)) {
                    if (a.name == name) la.Add(a);
                }
            }
            return la;
        }
     
        /// <summary>
        /// Gets Actor by name of a role.
        /// </summary>
        /// <param name="role">string representing name of a character</param>
        /// <returns>Actor or null if noone was found</returns>
        public static Actor GetActor(string role) {
            foreach (Series s in GetSeries()) {
                foreach (Actor a in GetActors(s.id)) {
                    if (a.role == role) return a;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds multiple Actors to any Series by TVDb id
        /// </summary>
        /// <param name="actors">What Actors to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddActor(int id , List<Actor> actors) {
            ActorAdder(id, actors);
        }

        /// <summary>
        /// Adds single Actor to any Series by TVDb id
        /// </summary>
        /// <param name="actor">What Actor to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddActor(int id, Actor actor) {
            ActorAdder(id, new List<Actor>() { actor });
        }

        /// <summary>
        /// Code for both overloads of AddActor
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="actors">List of Actors to add</param>
        private static void ActorAdder(int id, List<Actor> actors) {
            string original = ReadFromFile(db + id + "\\Actors.tvsp");
            List<Actor> list = new List<Actor>();
            if (!String.IsNullOrEmpty(original)) {
                JArray jo = JArray.Parse(original);
                list = jo.ToObject<List<Actor>>();
            }
            list.AddRange(actors);
            string json = JsonConvert.SerializeObject(list);
            WriteToFile(db + id + "\\Actors.tvsp", json);
        }


        /// <summary>
        /// Edits Actor in database
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="idActor">TVDb id of Actor</param>
        /// <param name="actor">new Actor that will replace old one</param>
        /// <returns>true if edit was successful or false if it was not</returns>
        public static bool EditActor(int id,int idActor, Actor actor) {
            if (RemoveActor(id, idActor)) {
                AddActor(id, actor);
                return true;
            }
            return false;
        }
       
        /// <summary>
        /// Removes Actor from database
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="idActor">TVDb id of Actor</param>
        /// <returns>true if removal was successful or false if it was not</returns>
        public static bool RemoveActor(int id, int idActor) {
            List<Actor> eps = GetActors(id);
            try {
                eps.Remove(eps.Single(se => se.id == idActor));
                string json = JsonConvert.SerializeObject(eps);
                WriteToFile(db + id + "\\Actors.tvsp", json);
                return true;
            } catch (Exception) {
                return false;
            }
        }
        #endregion

        #region Poster
       
        /// <summary>
        /// Returns all Posters by TVDb id
        /// </summary>
        /// <param name="id">TVDb id of Series </param>
        /// <returns></returns>
        public static List<Poster> GetPosters(int id) {
            string orig = ReadFromFile(db + id + "\\Posters.tvsp");
            List<Poster> eps = new List<Poster>();
            if (!String.IsNullOrEmpty(orig)) {
                JArray jo = JArray.Parse(orig);
                eps = jo.ToObject<List<Poster>>();
            }
            return eps;
        }
        
        /// <summary>
        /// Returns currently selected poster as BitmapImage so it can be set as Image.Source
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <returns>BitmapImage with poster in it or default image when no poster is downloaded</returns>
        public static BitmapImage GetSelectedPoster(int id) {
            Series s = GetSeries(id);
            if (s.defaultPoster == null) {
                List<Poster> lp = GetPosters(id);
                if (lp.Count > 0) {
                    s.defaultPoster = lp[0];
                    EditSeries(s.id, s);
                } else {
                    return App.Current.Resources["NoPoster"] as BitmapImage;
                }
            }
            string file = db + id + "\\Posters" + s.defaultPoster.fileName;
            if (File.Exists(file)) {
                return LoadImage(file);
            } else {
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://www.thetvdb.com/banners/"+s.defaultPoster.fileName, file);
                return LoadImage(file);
            }
        }

        /// <summary>
        /// Loads image from file
        /// </summary>
        /// <param name="file">what file to load from</param>
        /// <returns>loaded image</returns>
        private static BitmapImage LoadImage(string file) {
           return new BitmapImage(new Uri(file));
        }
        
        /// <summary>
        /// Adds multiple Posters to any Series by TVDb id
        /// </summary>
        /// <param name="posters">What Posters to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddPoster(int id,List<Poster> posters) {
            PosterAdder(id, posters);
        }

        /// <summary>
        /// Adds single poster to any Series by TVDb id
        /// </summary>
        /// <param name="poster">What Poster to add</param>
        /// <param name="id">TVDb id of Series</param>
        public static void AddPoster(int id, Poster poster) {
            PosterAdder(id, new List<Poster> { poster });
        }

        /// <summary>
        /// Code for both overloads of AddPoster()
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="posters">List of posters to add</param>
        private static void PosterAdder(int id, List<Poster> posters) {
            string original = ReadFromFile(db + id + "\\Posters.tvsp");
            List<Poster> list = new List<Poster>();
            if (!String.IsNullOrEmpty(original)) {
                JArray jo = JArray.Parse(original);
                list = jo.ToObject<List<Poster>>();
            }
            list.AddRange(posters);
            string json = JsonConvert.SerializeObject(list);
            WriteToFile(db + id + "\\Posters.tvsp", json);
        }

        /// <summary>
        /// Removes a Poster from the database
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="idPoster">TVDb id of Poster</param>
        /// <returns>true if removal was successful or false if it was not</returns>
        public static bool RemovePoster(int id, int idPoster) {
            List<Poster> eps = GetPosters(id);
            try {
                eps.Remove(eps.Single(se => se.id == idPoster));
                string json = JsonConvert.SerializeObject(eps);
                WriteToFile(db + id + "\\Posters.tvsp", json);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        /// <summary>
        /// Replaces a poster in a database
        /// </summary>
        /// <param name="id">TVDb id of Series</param>
        /// <param name="idPoster">TVDb id of Poster</param>
        /// <param name="poster">new Poster that replaces the old one</param>
        /// <returns>true if edit was successful or false if it was not</returns>
        public static bool EditPoster(int id, int idPoster, Poster poster) {
            if (RemovePoster(id, idPoster)) {
                AddPoster(id, poster);
                return true;
            }
            return false;
        }
        #endregion



        /// <summary>
        /// Reads from file, is safe - waits for file not to be used if it is used
        /// </summary>
        /// <param name="path">where to read from</param>
        /// <returns>read string</returns>
        public static string ReadFromFile(string path) {
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path)) File.Create(path).Dispose();         
            do {
                try {
                    if (!Directory.Exists(Path.GetDirectoryName(path))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    StreamReader sr = new StreamReader(path);
                    string text = sr.ReadToEnd();
                    sr.Close();
                    return text;
                } catch (IOException e) { }
                Thread.Sleep(10);
            } while (true);
        }

        /// <summary>
        /// Converts any object to json
        /// </summary>
        /// <param name="obj">object to convert</param>
        /// <returns>string formated as json</returns>
        private static string ObjectToJson(object obj) {
             return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Writes to file, is safe - if file is already used it waits for a bit
        /// </summary>
        /// <param name="path">where to write</param>
        /// <param name="json">what to write</param>
        public static void WriteToFile(string path, string json) {
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path)) File.Create(path).Dispose();
            do {
                try {
                    if (!Directory.Exists(Path.GetDirectoryName(path))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    StreamWriter sw = new StreamWriter(path);
                    sw.Write(json);
                    sw.Close();
                    return;
                } catch (IOException e) { }
                Thread.Sleep(10);

            } while (true);
        }

       
    }
}
