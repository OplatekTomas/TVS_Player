using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVS.API;
using static TVS.API.Episode;

namespace TVSPlayer {
    class Renamer {

        public static List<ScannedFile> FindAndRename(Series series) {
            List<ScannedFile> files = new List<ScannedFile>();
            FindAndRenameLibrary(series);
            return new List<ScannedFile>();
        }
        private static List<ScannedFile> FindAndRenameLibrary(Series series) {
            if (series.libraryPath == null) {
                int i = 1;
                while (true) {
                    string path = Settings.Library + "\\" + series.seriesName;
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                        series.libraryPath = path;
                        Database.EditSeries(series.id, series);
                        return new List<ScannedFile>();
                    } else {
                        path = Settings.Library + "\\" + series.seriesName + "_" + i;
                        i++;
                    }
                }
            } else {
                return new List<ScannedFile>();
            }
        }

        

    }
}
