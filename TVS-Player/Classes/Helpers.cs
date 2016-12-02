using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TVS_Player {
    static class Helpers {
        public static string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\";
        public static string showID;
        public static string showName;

        public static async Task<Tuple<string, string>>showSelector() {
            string idF = null, nameF = null;
            await Task.Run(() => {
                do {
                    idF = Helpers.showID;
                    nameF = Helpers.showName;
                    Thread.Sleep(100);
                } while (Helpers.showID == null && Helpers.showName == null);
                idF = Helpers.showID;
                nameF = Helpers.showName;
            });
            showID = null;
            showName = null;
            return new Tuple<string, string>(idF, nameF);


        }
    }
    }

