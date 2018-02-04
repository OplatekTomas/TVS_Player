using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Reflection;
using System.Security.Permissions;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;

namespace TVSPlayerUpdater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            if (!CanWrite()) {
                RunAsAdmin();
            } else {
                Update();
            }

        }

        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void RunAsAdmin() {
            Update();
        }


        private async void Update() {
            await Task.Run(() => {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0");
                wc.Headers.Add("Accept", "application/vnd.github.v3+json");
                var response = wc.DownloadString("https://api.github.com/repos/Kaharonus/TVS-Player/releases");
                var jo = JArray.Parse(response)[0]["assets"];
                List<Asset> assets = new List<Asset>();
                foreach (var token in jo) {
                    assets.Add(token.ToObject<Asset>());
                }
                WebClient downloadClient = new WebClient();
                downloadClient.DownloadProgressChanged += (s, ev) => ProgressChanged(ev);
                downloadClient.DownloadFileAsync(new Uri(assets[0].browser_download_url), Path.GetTempPath() + "\\TVSPlayerUpdate.TVSP");
                /*var asset = assets.Where(x => x.name.ToLower().Contains("standalone")).FirstOrDefault();
                if (asset != null) {
                    WebClient downloadClient = new WebClient();
                    downloadClient.DownloadProgressChanged += (s, ev) => ProgressChanged(ev.ProgressPercentage);
                    downloadClient.DownloadFileAsync(new Uri(asset.browser_download_url), Path.GetTempPath() + "\\TVSPlayerUpdate.TVSP");
                }*/
            });

        }

        private void ProgressChanged(DownloadProgressChangedEventArgs progress) {
            Dispatcher.Invoke(() => {
                Progress.Value = progress.ProgressPercentage;
            }, DispatcherPriority.Send);

        }

        private bool CanWrite() {
            try {
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(Assembly.GetExecutingAssembly().Location);
                return true;
            } catch (UnauthorizedAccessException) {
                return false;
            }
        }

    }

    public class Asset {
        public int id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public int size { get; set; }
        public string browser_download_url { get; set; }
    }
}
