using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TVS.API;

namespace TVSPlayer {
    class Helper {
        public static string data = @"C:\Users\Public\Documents\TVS-Player\";
        public static string posterLink = "https://www.thetvdb.com/banners/";


    }
    static class Extensions{
        public static void WaitAll(this IEnumerable<Task> tasks) {
            Task.WaitAll(tasks.ToArray());
        }
    }
}
