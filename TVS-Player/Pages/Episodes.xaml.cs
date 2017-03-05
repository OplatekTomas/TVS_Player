using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Timer = System.Timers.Timer;
using Image = System.Drawing.Image;
using System.Drawing;
using System.Globalization;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Reflection;
using Size = System.Windows.Size;
using System.Diagnostics;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Episodes.xaml
    /// </summary>
    public partial class Episodes : Page {
        int id;
        int season;
        Show ss;

        public Episodes(Show s, int season) {
            InitializeComponent();
            id = s.id;
            this.season = season;
            ss = s;
            ClickTimer = new Timer(300);
            ClickTimer.Elapsed += new ElapsedEventHandler(EvaluateClicks);
            Window m = Application.Current.MainWindow;
            ((MainWindow)m).SetTitle(s.name + " Season " + season);
        }
        public Episodes(Show s, int season,Episode e) {
            InitializeComponent();
            id = s.id;
            this.season = season;
            ss = s;
            ClickTimer = new Timer(300);
            ClickTimer.Elapsed += new ElapsedEventHandler(EvaluateClicks);
            ThreadForInfo(e);
        }
        private Timer ClickTimer;
        private int ClickCounter;
        private Episode lastEP;

        private void List_Loaded(object sender, RoutedEventArgs e) {
            Action a;
            a = () => LoadEP();
            Thread thread = new Thread(a.Invoke);
            thread.Start();

        }
        private void LoadEP() {
            List<Episode> temp = new List<Episode>();
            List<Episode> tempCheck = DatabaseEpisodes.ReadDb(id);
            foreach (Episode episode in tempCheck) {
                if (episode.season == season) {
                    temp.Add(episode);
                }
            }
            List<Episode> episodes = temp.OrderBy(ep => ep.episode).ToList();
            foreach (Episode episode in episodes) {
                string text;
                var ffProbe = new NReco.VideoInfo.FFProbe();
                if (episode.locations.Count > 0) {
                    try {
                        var videoInfo = ffProbe.GetMediaInfo(episode.locations[0]);
                        text = videoInfo.Duration.ToString(@"hh\:mm\:ss");
                    } catch (Exception) {
                        text = "--:--:--";
                    }
                } else {
                    text= "--:--:--";
                }
                if (episode.locations.Count > 0) {
                    Dispatcher.Invoke(new Action(() => {
                        EpisodeControl EPC = new EpisodeControl();
                        EPC.EPGrid.MouseLeftButtonDown += (s, ev) => ClickCheck(episode);
                        EPC.EpisodeName.Text = episode.name;
                        EPC.noEp.Text = getEPOrder(episode);
                        EPC.timerText.Text = text;
                        EPC.PlayEP.MouseLeftButtonDown += (s, ev) => PlayEP(HighestRes(episode), episode);
                        List.Children.Add(EPC);
                    }), DispatcherPriority.Send);
                } else {
                    Dispatcher.Invoke(new Action(() => {
                        Color c = Color.FromArgb(255, 68, 68, 68);
                        SolidColorBrush cb = new SolidColorBrush(ToMediaColor(c));
                        EpisodeControl EPC = new EpisodeControl();
                        EPC.EPGrid.MouseLeftButtonDown += (s, ev) => ThreadForInfo(episode);
                        EPC.EpisodeName.Text = episode.name;
                        EPC.EpisodeName.Foreground = cb;
                        EPC.noEp.Text = getEPOrder(episode);
                        EPC.noEp.Foreground = cb;
                        EPC.timerText.Text = text;
                        EPC.timerText.Foreground = cb;
                        EPC.lenghtText.Foreground = cb;
                        EPC.PlayEP.Source = Convert(new Bitmap(Properties.Resources.play_button_dark));
                        if (episode.release != "--.--.----") { 
                            if (DateTime.ParseExact(episode.release, "dd.MM.yyyy", null) < DateTime.Now) {
                                EPC.DownloadButton.Visibility = Visibility.Visible; 
                                EPC.DownloadButton.MouseLeftButtonDown += (s, ev) => DownloadOptions(ss.name, episode);
                            }
                        }
                        List.Children.Add(EPC);
                    }), DispatcherPriority.Send);
                }
            }
      
        }

        private void DownloadOptions(string name, Episode episode) {
            if (AppSettings.GetOneClick()) {
                TorrentItem torrent = FindTorrent.GetBestTorrent(name, episode.season, episode.episode, AppSettings.GetOneClickQuality());
                TorrentDownloader t = new TorrentDownloader(torrent);
                t.DownloadTorrent();
            } else { 
                List<TorrentItem> t = FindTorrent.GetTorrents(name, episode.season, episode.episode);
                string text = null;
                foreach (TorrentItem ti in t) {
                    text += ti.name + "\n";
                }
                MessageBox.Show(text);
            }
        }

        private string HighestRes(Episode e) {
            string path = null;
            int oldSize = 0;
            int currentSize = 0;
            var ffProbe = new NReco.VideoInfo.FFProbe();
            foreach (string p in e.locations) {
                var videoInfo = ffProbe.GetMediaInfo(p);
                var res = videoInfo.Streams;
                currentSize = res[0].Width;
                if (oldSize<currentSize) {
                    path = p;
                }
            }
            return path;
        }

        private System.Windows.Media.Color ToMediaColor(Color color) {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
        private void PlayEP(string path,Episode e) {
            if (!AppSettings.GetBuildInPlayer()) {
                Process.Start(path);
            } else { 
                Page showPage = new Player(path,e,ss);
                Window main = Window.GetWindow(this);
                ((MainWindow)main).AddTempFrameIndex(showPage);
                showPage.Focus();
            }
        }

        public static string getEPOrder(Episode e) {
            int season = e.season;
            int episode = e.episode;
            if(season < 10) {
                if (episode < 10) {
                    return "S0" + season + "E0" + episode;
                } else if (episode >= 10) {
                    return "S0" + season + "E" + episode;
                }
            } else if (season >= 10) {
                if (episode < 10) {
                    return "S" + season + "E0" + episode;
                } else if (episode >= 10) {
                    return "S" + season + "E" + episode;
                }
            }
            return null;
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Page showPage = new Seasons(ss);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }

        private void ClickCheck(Episode e) {
            lastEP = e;
            ClickTimer.Stop();
            ClickCounter++;
            ClickTimer.Start();
        }
        private void EvaluateClicks(object source, ElapsedEventArgs e) {
            Episode episode = lastEP;
            ClickTimer.Stop();
            switch (ClickCounter){
                case 1:
                    Dispatcher.Invoke(new Action(() => {
                        ThreadForInfo(episode);
                    }), DispatcherPriority.Send);
                    break;
                case 2:
                    Dispatcher.Invoke(new Action(() => {
                        PlayEP(episode.locations[0], episode);
                    }), DispatcherPriority.Send);
                    break;
            }
            ClickCounter = 0;
        }

        private void ThreadForInfo(Episode e) {
            Action a;
            a = () => SetInfo(e);
            Thread t = new Thread(a.Invoke);
            t.Name = "Episode Detail";
            t.Start();
        }

        private void SetInfo(Episode episode) {
            JObject jo = JObject.Parse(Api.EPInfo(episode.id));
            string directors = "";
            string writers = "";
            Action p = () => CreatePic("http://thetvdb.com/banners/" + jo["data"]["filename"].ToString());
            Thread setPicutre = new Thread(p.Invoke);
            setPicutre.Start();
            Dispatcher.Invoke(new Action(() => {
                foreach (JToken JT in jo["data"]["directors"]) {
                    directors += JT.ToString() + ", ";
                }
                System.Windows.GridLength gl = new GridLength(1,GridUnitType.Star);
                Base.ColumnDefinitions[1].Width = gl;
                foreach (JToken JT in jo["data"]["writers"]) {
                    writers += JT.ToString() + ", ";
                }
                EPName.Text = episode.name;
                ShowName.Text = ss.name;
                if (jo["data"]["firstAired"].ToString() != "") {
                    DateTime dt = DateTime.ParseExact(jo["data"]["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    FirstAired.Text = dt.ToString("dd.MM.yyyy");
                    if (dt.AddDays(1) > DateTime.Now) {
                        Director.Text = "-";
                        Writer.Text = "-";
                        Overview.Text = "-";
                    } else {
                       Director.Text = directors.Remove(directors.Length - 2, 2);
                        if (writers.Length > 0) {
                            Writer.Text = writers.Remove(writers.Length - 2, 2);
                        }
                        Overview.Text = jo["data"]["overview"].ToString();
                    }
                } else {
                    FirstAired.Text = "--.--.----";
                    Director.Text = "-";
                    Writer.Text = "-";
                    Overview.Text = "-";
                }
                
            }), DispatcherPriority.Send);
        }

        private void CreatePic(string url) {         
            Dispatcher.Invoke(new Action(() => {
                BitmapSource bmp = setThumbnail(url);
                thumbnail.Source = bmp;
            }), DispatcherPriority.Send);
        }

        private BitmapSource setThumbnail(string url) {
            WebClient wc = new WebClient();
            try {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData(url))) {
                    var imageSource = new BitmapImage();
                    Image img = Image.FromStream(stream);
                    BitmapSource bmp = GetImageStream(img);
                    return bmp;
                }
            } catch (WebException) {
                return null;
            }

        }
        public BitmapImage Convert(Bitmap bitmap) {
            using (MemoryStream stream = new MemoryStream()) {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        public static BitmapSource GetImageStream(Image myImage) {
            var bitmap = new Bitmap(myImage);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }
    }
}
