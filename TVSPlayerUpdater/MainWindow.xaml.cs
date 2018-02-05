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
using System.Windows.Threading;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Collections;

namespace TVSPlayerUpdater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            if (false) {
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
            } else {
                Test();
            }
        }

        private async void Test() {
            await Update();
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



        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void RunAsAdmin() {
            Update();
        }

        private async Task Update() {
            await Task.Run(() => {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0");
                wc.Headers.Add("Accept", "application/vnd.github.v3+json");
                var response = wc.DownloadString("https://api.github.com/repos/Kaharonus/TVS-Player/releases");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var unParsed = serializer.Deserialize<List<Dictionary<string, object>>>(response)[0]["assets"];
                var dictionary = ((ArrayList)unParsed).Cast<Dictionary<string,object>>().ToList().Where(x=>x["name"].ToString().Contains("standalone")).FirstOrDefault();
                if (dictionary != null) {
                    string url = dictionary["browser_download_url"].ToString();
                    WebClient downloadClient = new WebClient();
                    downloadClient.DownloadProgressChanged += (s, ev) => ProgressChanged(ev);
                    var path = Path.GetTempPath() + "\\TVSPlayerUpdate.tvsp";
                    downloadClient.DownloadFileCompleted += (s, ev) => DownloadCompleted(path);
                    downloadClient.DownloadFileAsync(new Uri(url), path);
                }
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
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var list = serializer.Deserialize<List<string[]>>(sr.ReadToEnd());
            var item = list.Where(x => x[0] == "lastUpdate").FirstOrDefault();
            int index = list.IndexOf(item);
            item[1] = DateTime.Now.ToString();
            list[index] = item;
            string json = serializer.Serialize(list).Replace(@"\\", @"\");
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
}
