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
namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowInfoSmall.xaml
    /// </summary>
    public partial class ShowInfoSmall : Page {
        string info;
        public ShowInfoSmall(string inf) {
            InitializeComponent();
            info = inf;
            setBanner();
        }
        private void setBanner() {
            JObject banner = JObject.Parse(info);
            var bannerPic = banner["banner"];
            WebClient wc = new WebClient();
            try {
                using (MemoryStream stream = new MemoryStream(wc.DownloadData("http://thetvdb.com/banners/" + bannerPic.ToString()))) {
                    var imageSource = new BitmapImage();
                    System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                    image.Source = GetImageStream(img);
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
    }
}
