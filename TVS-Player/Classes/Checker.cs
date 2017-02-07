using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TVS_Player {
    class Checker {
        public static void UpdateShow(int id) {
            Action check;
            check = () => Update(id);
            Thread threadCheck = new Thread(check.Invoke);
            threadCheck.Name = "Checking for updates";
            threadCheck.Start();
        }
        public static void UpdateShowFull(int id) {
            Action check;
            check = () => UpdateFull(id);
            Thread threadCheck = new Thread(check.Invoke);
            threadCheck.Name = "Checking for updates";
            threadCheck.Start();
        }

        private static void Update(int id) {
            List<Episode> EPList = DatabaseEpisodes.readDb(id);
            int season = EPList.Max(y => y.season);
            string seasonInfo = Api.apiGetEpisodesBySeasons(id, season);
            string nextSeason = Api.apiGetEpisodesBySeasons(id, season+1);
            if (nextSeason != null) {
                JObject jo = JObject.Parse(nextSeason);
                foreach (JToken jt in jo["data"]) {
                    DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-mm-dd", CultureInfo.InvariantCulture);
                    EPList.Add(new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), dt.ToString("dd.mm.yyyy"), false, new List<string>()));
                }
            }
            JObject jo2 = JObject.Parse(seasonInfo);
            foreach (JToken jt in jo2["data"]) {
                DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                int episode = Int32.Parse(jt["airedEpisodeNumber"].ToString());
                int index = EPList.FindIndex(s =>s.season == season && s.episode == episode);            
                EPList[index] = new Episode(jt["episodeName"].ToString(),season,episode, Int32.Parse(jt["id"].ToString()), dt.ToString("dd.MM.yyyy"), EPList[index].downloaded, EPList[index].locations);
            }
            DatabaseEpisodes.createDB(id,EPList);
        }
        private static void UpdateFull(int id) {
            List<Episode> epi = DatabaseEpisodes.readDb(id);
            for (int i = 1; i <= Renamer.GetNumberOfSeasons(id); i++) {
                JObject jo = JObject.Parse(Api.apiGetEpisodesBySeasons(id, i));
                foreach (JToken jt in jo["data"]) {
                    DateTime dt = DateTime.ParseExact(jt["firstAired"].ToString(), "yyyy-mm-dd", CultureInfo.InvariantCulture);
                    int episode = Int32.Parse(jt["airedEpisodeNumber"].ToString());
                    try {
                        int index = epi.FindIndex(s => s.season == i && s.episode == episode);
                        epi[index] = new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), dt.ToString("dd.mm.yyyy"), epi[index].downloaded, epi[index].locations);
                    } catch (ArgumentNullException) {
                        epi.Add(new Episode(jt["episodeName"].ToString(), Int32.Parse(jt["airedSeason"].ToString()), Int32.Parse(jt["airedEpisodeNumber"].ToString()), Int32.Parse(jt["id"].ToString()), dt.ToString("dd.mm.yyyy"), false, new List<string>()));
                    }
                }
            }
            DatabaseEpisodes.createDB(id, epi);
        }

    }
}
