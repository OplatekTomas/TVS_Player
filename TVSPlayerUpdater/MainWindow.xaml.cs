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
using System.Security.AccessControl;

namespace TVSPlayerUpdater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            if (true) {
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
            await Update("");
        }

        private async void RunUpdate(string path) {
            if (!Debugger.IsAttached) {
                //Debugger.Launch();
            }
            Dispatcher.Invoke(() => { CurrentText.Text = "Downloading Update"; });
            if (!CanWrite(Path.GetDirectoryName(path))) {
                RunAsAdmin(path);
            } else {
                await Update(path);
            }
        }

        private void RunCopy() {
            var path = Path.GetTempPath() + "\\TVSPlayerUpdater.exe";
            if (File.Exists(path)) {
                File.Delete(path);
            }
            File.Copy(Assembly.GetExecutingAssembly().Location, path);
            Process.Start(path, "-Update \"" + Assembly.GetExecutingAssembly().Location+"\"");
            Environment.Exit(0);
        }

        private void RunClean() {   
            var path = Path.GetTempPath() + "\\TVSPlayerUpdater.exe";
            if (File.Exists(path)) {
                File.Delete(path);
            }
            Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\TVSPlayer.exe");
            Close();
        }



        private async void RunAsAdmin(string path) {
            Process p = new Process();
            p.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
            p.StartInfo.Verb = "runas";
            p.StartInfo.Arguments = "-Update \"" + path + "\"";
            p.Start();
            Close();
        }

        private async Task Update(string path) {
            this.Activate();
            await Task.Run(() => {
                WebClient wc = new WebClient();
                wc.Headers.Add("user-agent", "TVS-Player/alpha_build");
                wc.Headers.Add("Accept", "application/vnd.github.v3+json");
                var response = wc.DownloadString("https://api.github.com/repos/Kaharonus/TVS-Player/releases");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var unParsed = serializer.Deserialize<List<Dictionary<string, object>>>(response).OrderByDescending(x=>x["published_at"]).ToList()[0]["assets"];
                var dictionary = ((ArrayList)unParsed).Cast<Dictionary<string,object>>().ToList().Where(x=>x["name"].ToString().ToLower().Contains("standalone")).FirstOrDefault();
                if (dictionary != null) {
                    string url = dictionary["browser_download_url"].ToString();
                    WebClient downloadClient = new WebClient();
                    downloadClient.DownloadProgressChanged += (s, ev) => ProgressChanged(ev);
                    downloadClient.DownloadFileCompleted += (s, ev) => DownloadCompleted(path);
                    downloadClient.DownloadFileAsync(new Uri(url), Path.GetTempPath() + "\\TVSPlayerUpdate.zip");
                }
            });

        }

        private void DownloadCompleted(string orig) {          
            Dispatcher.Invoke(() => { CurrentText.Text = "Extracting update"; });
            //MessageBox.Show("...");
            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(orig));
            foreach (FileInfo file in di.GetFiles()) {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories()) {
                dir.Delete(true);
            }
            ZipFile.ExtractToDirectory(Path.GetTempPath() + "\\TVSPlayerUpdate.zip", Path.GetDirectoryName(orig));
            File.Delete(Path.GetTempPath() + "\\TVSPlayerUpdate.zip");
            EditSettings();
            Process.Start(orig, "-Clean");
            Dispatcher.Invoke(() => {
                Close();
            });
        }

        private void ProgressChanged(DownloadProgressChangedEventArgs progress) {
            Dispatcher.Invoke(() => {
                StatsText.Text = progress.ProgressPercentage + "%";
                Progress.Value = progress.ProgressPercentage;
            }, DispatcherPriority.Send);

        }

        private void EditSettings() {
            var settingsPath = @"C:\Users\Public\Documents\TVS-Player\Settings.tvsp";
            StreamReader sr = new StreamReader(settingsPath);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string temp = sr.ReadToEnd();
            sr.Close();
            var list = serializer.Deserialize<List<string[]>>(temp);
            var onStartup = list.Where(x => x[0] == "updateOnStartup").FirstOrDefault();
            int indexStartup = list.IndexOf(onStartup);
            onStartup[1] = "False";
            list[indexStartup] = onStartup;
            string json = serializer.Serialize(list);
            File.WriteAllText(settingsPath, json);
        }

        public static bool CanWrite(string destDir) {
            try {
                byte[] data = new byte[5 * 1024 * 1024];
                Random rng = new Random();
                rng.NextBytes(data);
                File.WriteAllBytes(destDir + "\\AdminTest.tvsp", data);
                return true;
            } catch (UnauthorizedAccessException e) {
                return false;
            }
            
        }

    }
}
