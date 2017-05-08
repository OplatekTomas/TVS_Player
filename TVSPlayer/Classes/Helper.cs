using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TVSPlayer {
    class Helper {

        //DO NOT USE THIS VARIABLE OUTSIDE FUNCTION SearchShowAsync() (MainWindow) OR FROM FUNCTION ReturnTVShowWhenNotNull() THAT IS RIGHT F*CKING HERE!
        public static TVShow show = null;

        //Asynchronous task that waits until variable show is not null and then returns this variable
        public static async Task<TVShow> ReturnTVShowWhenNotNull() {
            TVShow s = null;
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
