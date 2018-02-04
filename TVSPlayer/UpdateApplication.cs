using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TVSPlayer {
    class UpdateApplication {
        public async static Task CheckForUpdates() {
            await Task.Run(() => {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0");
                wc.Headers.Add("Accept", "application/vnd.github.v3+json");
                var response = wc.DownloadString("https://api.github.com/repos/Kaharonus/TVS-Player/releases");
                var serverTime = wc.ResponseHeaders["Date"];
                JArray jo = JArray.Parse(response);
                if (Settings.LastUpdate < DateTime.Parse(wc.ResponseHeaders["Date"])) {
                   // Settings.UpdateOnStartup = true;
                }
            });

        }

        public static void StartUpdate() {
            Settings.LastUpdate = DateTime.Now;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TVSPlayerUpdater.exe";
            Process.Start(path);
            Application.Current.Shutdown();

        }

    }
}
