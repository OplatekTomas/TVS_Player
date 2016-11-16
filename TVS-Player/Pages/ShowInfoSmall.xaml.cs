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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowInfoSmall.xaml
    /// </summary>
    public partial class ShowInfoSmall : Page {
        string info;
        public ShowInfoSmall(string inf) {
            InitializeComponent();
            info = inf;
            fillLayout();
        }
        int actorNumber = 1;
        private void fillLayout() {
            Action setB;
            setB = () => inThread();
            Thread banner = new Thread(setB.Invoke);
            banner.Start();
            JObject parse = JObject.Parse(info);
            for (int i = 0; i < parse["data"]["genre"].Count(); i++) {
                if (i == 0) {
                    genre.Text += parse["data"]["genre"][i].ToString();
                } else { 
                    genre.Text += ", "+ parse["data"]["genre"][i].ToString();
                }
            }
            showName.Text = parse["data"]["seriesName"].ToString();
            status.Text = parse["data"]["status"].ToString();
            network.Text = parse["data"]["network"].ToString();
            epLenght.Text = parse["data"]["runtime"].ToString();
            airTime.Text = parse["data"]["airsDayOfWeek"].ToString() + " at " + parse["data"]["airsTime"].ToString();
            overview.Text = parse["data"]["overview"].ToString();
            try {
                DateTime dt = DateTime.ParseExact(parse["data"]["firstAired"].ToString(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                firstAir.Text = dt.ToString("dd.MM.yyyy");
            } catch (Exception e) {
                firstAir.Text = ("");
            }
        }
        private void inThread() {
            JObject jo = JObject.Parse(info);
            setBanner(jo);
            string actorInfo = Api.apiGetActors(Int32.Parse(jo["data"]["id"].ToString()));
            JObject actorInfoJ = JObject.Parse(actorInfo);
            for (int i = 0; i < actorInfoJ["data"].Count(); i++) {
                if (Int32.Parse(actorInfoJ["data"][i]["sortOrder"].ToString()) == 0) {
                    setActor(actorNumber,actorInfoJ["data"][i]["image"].ToString(),jo);
                }
            }
        }
        private void setBanner(JObject banner) {
            var bannerPic = banner["data"]["banner"];
            WebClient wc = new WebClient();
            try {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData("http://thetvdb.com/banners/" + bannerPic.ToString()))) {
                    var imageSource = new BitmapImage();
                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    Dispatcher.Invoke(new Action(() => {
                        image.Source = GetImageStream(img);
                    }));

                }
            } catch (WebException) { }
        }
        public static BitmapSource GetImageStream(System.Drawing.Image myImage) {
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
        private void setActor(int number,string link,JObject info) {
            WebClient wc = new WebClient();
            try {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData("http://thetvdb.com/banners/" + link))) {
                    var imageSource = new BitmapImage();
                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    Dispatcher.Invoke(new Action(() => {
                        switch (number) {
                            case 1:
                                actorPic1.Source = GetImageStream(img);
                                actorNumber++;
                                break;
                            case 2:
                                actorPic2.Source = GetImageStream(img);
                                actorNumber++;
                                break;
                            case 3:
                                actorPic3.Source = GetImageStream(img);
                                break;
                        }
                    }));
                }
            } catch (WebException) { }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
    }
}
