using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Shapes;
using System.Drawing;
using System.Threading;
using Image = System.Drawing.Image;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowInfoSmall.xaml
    /// </summary>
    public partial class ShowInfoSmall : Page {
        Show show;
        public ShowInfoSmall(Show s) {
            InitializeComponent();
            show = s;
            fillLayout();
        }
        private void fillLayout() {
            Action setB = () => inThread();
            Thread banner = new Thread(setB.Invoke);
            banner.Start();
            foreach (string genreText in show.genre) {
                genre.Text += genreText;
            }
            showName.Text = show.name;
            status.Text = show.status;
            epLenght.Text = show.EPlenght.ToString();
            airTime.Text = show.airtime;
            overview.Text = show.overview;
            rating.Text = show.rating+"/10";
        }
        private void inThread() {
            try {
                setLeft(show.actors[0]);
                setRight(show.actors[2]);
                setMiddle(show.actors[1]);
            } catch (Exception) { }
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
            ((MainWindow)main).CloseTempFrameIndex();
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
            ((MainWindow)main).CloseTempFrameIndex();
        }
        private void setLeft(Show.ActorInfo actor) {
            string name = actor.name;
            string character = actor.character;
            string url = actor.link;
            Dispatcher.Invoke(new Action(() => {
                f.Visibility = Visibility.Visible;
                f.actorPic.Source = getImage(url);
                f.ActorName.Text = name;
                f.CharacterName.Content = character;
                f.ActorName.MouseUp += (s, e) => openActor(actor.name);

            }));
        }
        private void setRight(Show.ActorInfo actor) {
            string name = actor.name;
            string character = actor.character;
            string url = actor.link;
            Dispatcher.Invoke(new Action(() => {
                t.Visibility = Visibility.Visible;
                t.actorPic.Source = getImage(url);
                t.ActorName.Text = name;
                t.CharacterName.Content = character;
                t.ActorName.MouseUp += (s, e) => openActor(actor.name);

            }));
        }
        private void setMiddle(Show.ActorInfo actor) {
            string name = actor.name;
            string character = actor.character;
            string url = actor.link;
            Dispatcher.Invoke(new Action(() => {
                s.Visibility = Visibility.Visible;
                s.actorPic.Source = getImage(url);
                s.ActorName.Text = name;
                s.CharacterName.Content = character;
                s.ActorName.MouseUp += (s, e) => openActor(actor.name);

            }));
        }
        private void openActor(string name) {
            System.Diagnostics.Process.Start("http://www.imdb.com/find?s=all&q=+"+name.Replace(" ","+")+"&ref_=nv_sr_sm");
        }

    }
}
