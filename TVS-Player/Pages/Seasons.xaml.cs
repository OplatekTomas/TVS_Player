using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Image = System.Drawing.Image;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Seasons.xaml
    /// </summary>
    public partial class Seasons : Page {
        private SelectedShows selectedShow;
        public Seasons(SelectedShows ss) {
            InitializeComponent();
            selectedShow = ss;
            StartUp();
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Page showPage = new Shows();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }

        private void StartUp() {
            int seasons = DatabaseAPI.GetSeasons(Int32.Parse(selectedShow.idSel));
            for (int i = 1; i <= seasons; i++) {
                SeasonControl se = CreateControl(i);
                List.Children.Add(se);
            }
            Action a;
            a = () => FillLayoutDownload();
            Thread t = new Thread(a.Invoke);
            t.Start();
        }

        private void FillLayoutDownload() {
            string info = Api.apiGet(Int32.Parse(selectedShow.idSel));
            fillLayout(info);
        }

        private void ShowSeason(int season) {
            Page ep = new Episodes(selectedShow,season);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(ep);
        }
        private SeasonControl CreateControl(int season) {
            SeasonControl se = new SeasonControl();
            se.seasonText.Text = "Season " + season;
            se.noEp.Text = DatabaseEpisodes.GetEpPerSeason(Int32.Parse(selectedShow.idSel), season).ToString();
            se.SeasonGrid.MouseDown += (s, e) => { ShowSeason(season); };
            se.releaseDate.Text = DatabaseEpisodes.GetSeasonRelease(Int32.Parse(selectedShow.idSel), season);
            se.Height = 51;
            return se;
        }
        private void fillLayout(string info) {
            Action setB;
            setB = () => inThread(info);
            Thread banner = new Thread(setB.Invoke);
            banner.Start();
            JObject parse = JObject.Parse(info);
            Dispatcher.Invoke(new Action(() => {
                for (int i = 0; i < parse["data"]["genre"].Count(); i++) {
                    if (i == 0) {
                        genre.Text += parse["data"]["genre"][i].ToString();
                    } else {
                        genre.Text += ", " + parse["data"]["genre"][i].ToString();
                    }
                }
                showName.MouseLeftButtonDown += (s, e) => OpenImdb(parse["data"]["imdbId"].ToString()); 
                showName.Text = parse["data"]["seriesName"].ToString();
                status.Text = parse["data"]["status"].ToString();
                network.Text = parse["data"]["network"].ToString();
                epLenght.Text = parse["data"]["runtime"].ToString();
                airTime.Text = parse["data"]["airsDayOfWeek"].ToString() + " at " + parse["data"]["airsTime"].ToString();
                overview.Text = parse["data"]["overview"].ToString();
                rating.Text = parse["data"]["siteRating"].ToString() + "/10";
                try {
                    DateTime dt = DateTime.ParseExact(parse["data"]["firstAired"].ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    firstAir.Text = dt.ToString("dd.MM.yyyy");
                } catch (Exception) {
                    firstAir.Text = ("");
                }
            }));
        }

        private void OpenImdb(string imdbID) {
            System.Diagnostics.Process.Start("http://www.imdb.com/find?s=all&q=+" + imdbID + "&ref_=nv_sr_sm");
        }

        private void inThread(string info) {
            JObject jo = JObject.Parse(info);
            setBanner(jo);
            string actorInfo = Api.apiGetActors(Int32.Parse(jo["data"]["id"].ToString()));
            JObject actorInfoJ = JObject.Parse(actorInfo);
            chooseActors(actorInfoJ);
        }
        struct Actor {
            public string name;
            public string character;
            public string url;
            public int role;
            public Actor(string nameI, string characterI, string urlI, int roleI) {
                name = nameI;
                character = characterI;
                url = urlI;
                role = roleI;

            }
        }
        private void chooseActors(JObject info) {
            List<Actor> actors = new List<Actor>();
            List<Actor> selectedAc = new List<Actor>();
            for (int i = 0; i < info["data"].Count(); i++) {
                actors.Add(new Actor(info["data"][i]["name"].ToString(), info["data"][i]["role"].ToString(), info["data"][i]["image"].ToString(), Int32.Parse(info["data"][i]["sortOrder"].ToString())));
            }
            for (int i = 0; i < actors.Count(); i++) {
                if (actors[i].role == 0) {
                    selectedAc.Add(actors[i]);
                }
            }
            if (selectedAc.Count() < 3) {
                int role = 1;
                do {
                    for (int i = 0; i < actors.Count(); i++) {
                        if (actors[i].role == role) {
                            if (selectedAc.Count() == 3) {
                                break;
                            }
                            selectedAc.Add(actors[i]);
                        }
                    }
                    role++;
                    if (role >= 4) {
                        selectedAc.Add(new Actor(null, null, null, 0));
                    }
                } while (selectedAc.Count < 3);
            }
            try {
                setLeft(selectedAc[0]);
                setRight(selectedAc[2]);
                setMiddle(selectedAc[1]);
            } catch (ArgumentNullException) { }

        }


        private void setBanner(JObject banner) {
            var bannerPic = banner["data"]["banner"];
            WebClient wc = new WebClient();
            try {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData("http://thetvdb.com/banners/" + bannerPic.ToString()))) {
                    var imageSource = new BitmapImage();
                    Image img = Image.FromStream(stream);
                    Dispatcher.Invoke(new Action(() => {
                        image.Source = GetImageStream(img);
                    }));

                }
            } catch (WebException) { }
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

        private void button_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
        private BitmapSource getImage(string url) {
            WebClient wc = new WebClient();
            try {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData("http://thetvdb.com/banners/" + url))) {
                    var imageSource = new BitmapImage();
                    Image img = Image.FromStream(stream);
                    return GetImageStream(img);
                }
            } catch (WebException) { return null; }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
        private void setLeft(Actor actor) {
            string name = actor.name;
            string character = actor.character;
            string url = actor.url;
            Dispatcher.Invoke(new Action(() => {
                f.Visibility = Visibility.Visible;
                f.actorPic.Source = getImage(url);
                f.ActorName.Text = name;
                f.CharacterName.Content = character;
                f.ActorName.MouseUp += (s, e) => openActor(actor.name);

            }));
        }
        private void setRight(Actor actor) {
            string name = actor.name;
            string character = actor.character;
            string url = actor.url;
            Dispatcher.Invoke(new Action(() => {
                t.Visibility = Visibility.Visible;
                t.actorPic.Source = getImage(url);
                t.ActorName.Text = name;
                t.CharacterName.Content = character;
                t.ActorName.MouseUp += (s, e) => openActor(actor.name);

            }));
        }
        private void setMiddle(Actor actor) {
            string name = actor.name;
            string character = actor.character;
            string url = actor.url;
            Dispatcher.Invoke(new Action(() => {
                s.Visibility = Visibility.Visible;
                s.actorPic.Source = getImage(url);
                s.ActorName.Text = name;
                s.CharacterName.Content = character;
                s.ActorName.MouseUp += (s, e) => openActor(actor.name);

            }));
        }
        private void openActor(string name) {
            System.Diagnostics.Process.Start("http://www.imdb.com/find?s=all&q=+" + name.Replace(" ", "+") + "&ref_=nv_sr_sm");
        }
        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e) {

        }


    }
}
