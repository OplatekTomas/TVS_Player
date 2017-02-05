using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TVS_Player {
    class Checker {
        public static void CheckForUpdates(int id) {
            List<Episode> EPList = DatabaseEpisodes.readDb(id);
            int season = EPList.Max(y => y.season);
            int episode = EPList.FindAll(e => e.season == season).Max(x => x.episode);
            Action check;
            check = () => Check(season,episode,id,EPList);
            Thread threadCheck = new Thread(check.Invoke);
            threadCheck.Name = "Checking for updates";
            threadCheck.Start();
        }

        private static void Check(int season, int episode,int id,List<Episode> EPList) {
            episode = episode+1;
            string nextEP = Api.apiGet(season, episode, id);
            string nextSeason = Api.apiGet(season + 1, 1, id);
            if (nextEP != null) {
                JObject jo = JObject.Parse(nextEP);
                 
                //Episode e = new Episode();
                //DatabaseEpisodes.addEPToDb(e);
            }
            if (nextSeason != null) {

            }
        }

        public static void CheckDBUpdate() {
            foreach (SelectedShows selected in DatabaseAPI.database.Shows) {
                CheckForUpdates(Int32.Parse(selected.idSel));
            }
        }


    }
}
