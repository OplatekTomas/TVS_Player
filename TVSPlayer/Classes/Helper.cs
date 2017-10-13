using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TVS.API;

namespace TVSPlayer
{
    class Helper {
        public static string data = @"C:\Users\Public\Documents\TVS-Player";
        public static string posterLink = "https://www.thetvdb.com/banners/";

        /// <summary>
        /// DO NOT USE THIS VARIABLE OUTSIDE FUNCTION SearchShowAsync() (MainWindow) OR FROM FUNCTION ReturnTVShowWhenNotNull().
        /// </summary>
        public static Series show = null;
        /// <summary>
        /// Asynchronous task that waits until variable show is not null and then returns this variable
        /// Can be only used in void or Task<TVShow> functions
        /// </summary>
        public static async Task<Series> ReturnTVShowWhenNotNull() {
            Series s = null;
            await Task.Run(() => {
                do {
                    s = show;
                    Thread.Sleep(100);
                } while (show == null);

                s = show;
            });
            show = null;
            return s;
        }
    }
}
