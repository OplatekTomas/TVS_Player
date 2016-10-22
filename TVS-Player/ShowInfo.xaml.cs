using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowInfo.xaml
    /// </summary>
    public partial class ShowInfo : Page {

        public ShowInfo() {
            InitializeComponent();
        }

        public ShowInfo(int ID) {
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //downloads all images
            /*path += "\\TVS-Player\\" + ID.ToString() + "\\";
            InitializeComponent();
            JObject posters = JObject.Parse(Api.apiGetAllPosters(ID));
            foreach (JToken jtok in posters["data"]) {
                string pathToImage = jtok["fileName"].ToString();
                string url = "http://thetvdb.com/banners/" + pathToImage;
                using (WebClient client = new WebClient())
                    client.DownloadFile(new Uri(url), path + jtok["id"] + ".jpg");
            }
            */
            JObject jo = JObject.Parse(Api.apiGet(ID));
            JmenoSerialu.Content = jo["data"]["seriesName"].ToString();
            Popisek.Text = jo["data"]["overview"].ToString();
            Rok.Text = jo["data"]["firstAired"].ToString();
            Api.apiGetPoster(ID);
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\TVS-Player\\" + ID.ToString() + "\\" + ID.ToString() + ".jpg";
            Obrazek.Source = new BitmapImage(new Uri(path));
        }

        private void ReturnBack_Event(object sender, MouseButtonEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView((Page)new Shows());
        }
    }
}
