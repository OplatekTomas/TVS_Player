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
using System.IO.Compression;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Diagnostics;

namespace TVSPlayerUpdater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            bool update = false;
            string path = null;
            bool clean = false;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i != args.Length; ++i) {
                if (args[i].Contains("-Update")) {
                    update = true;
                    path = args[i + 1];
                }
                if (args[i] == "-Clean") {
                    clean = true;
                }
            }
            if (update) {
                RunUpdate(path);
            } else if (clean) {
                RunClean();
            } else {
                RunCopy();
            }
        }

        private void RunUpdate(string path) {
            /*if (!CanWrite()) {
                RunAsAdmin();
            } else {
                Update();
            }*/
            MessageBox.Show("Update");
            Process.Start(path, "-Clean");
            Close();
        }

        private void RunCopy() {
            var path = Path.GetTempPath() + "\\TVSPlayerUpdater.exe";
            if (File.Exists(path)) {
                File.Delete(path);
            }
            MessageBox.Show("Copy");

            File.Copy(Assembly.GetExecutingAssembly().Location, path);
            Process.Start(path, "-Update \"" + Assembly.GetExecutingAssembly().Location+"\"");
            Close();
        }

        private void RunClean() {
            MessageBox.Show("Clean");
            var path = Path.GetTempPath() + "\\TVSPlayerUpdater.exe";
            if (File.Exists(path)) {
                File.Delete(path);
            }
            Close();
        }

        private void test() {
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
                var path = Path.GetTempPath() + "\\TVSPlayerUpdate.tvsp";
                downloadClient.DownloadFileCompleted += (s, ev) => DownloadCompleted(path);
                downloadClient.DownloadFileAsync(new Uri(assets[0].browser_download_url), path);
            });

        }

        private void DownloadCompleted(string path) {
            string dir = Path.GetTempPath() + "\\TVSPlayerUpdate";
            Directory.CreateDirectory(dir);
            ZipFile.ExtractToDirectory(path, dir);
            foreach (var file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)) {
                try {
                    File.Move(file, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.GetFileName(file));
                } catch (Exception e) {

                }
            }
            Directory.Delete(dir);
            File.Delete(path);
            var settingsPath = @"C:\Users\Public\Documents\TVS-Player\Settings.tvsp";
            StreamReader sr = new StreamReader(settingsPath);
            var array = JArray.Parse(sr.ReadToEnd());
            var lastUpdate = array.Where(x => x[0].ToString() == "lastUpdate").FirstOrDefault();
            int index = array.IndexOf(lastUpdate);
            lastUpdate[1] = DateTime.Now;
            array[15] = lastUpdate;
            sr.Close();
            var json = JsonConvert.SerializeObject(array);
            File.WriteAllText(settingsPath, json);
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
