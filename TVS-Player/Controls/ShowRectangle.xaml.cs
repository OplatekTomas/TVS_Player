using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowRectangle.xaml
    /// </summary>
    public partial class ShowRectangle : Grid {
        public int ID = 121361;
        public string ShowName = "Game of Thrones";
        public bool Disabled = false;
        public string pathToImage;
        public ShowRectangle() {
            InitializeComponent();
            Api.apiGetPoster(ID);
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\TVS-Player\\" + ID.ToString() + "\\" + ID.ToString() + ".jpg";
            pathToImage = path;
            Image.Source = new BitmapImage(new Uri(path));
        }

        private void ShowClicked_Event(object sender, MouseButtonEventArgs e) {
            Page showPage = new ShowInfo(ID,this);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }

        public void GenerateName() {
            string json = Api.apiGet(ID);
            JObject info = JObject.Parse(json);
            ShowName = info["data"]["seriesName"].ToString();
        }
        public void SearchDisable() {
            if (!Disabled) {
                Storyboard sb = this.FindResource("DisableSearch") as Storyboard;
                sb.Begin();
                Disabled = true;
            }
        }
        public void SearchEnable() {
            if(Disabled){
                Storyboard sb = this.FindResource("EnableSearch") as Storyboard;
                sb.Begin();
                Disabled = false;
            }
        }

        private void RenameShow_Event(object sender, RoutedEventArgs e) {
            Page showPage = new RenameShow(this);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }

        private void ChooseImage_Event(object sender, RoutedEventArgs e) {
            Page showPage = new SelectShowPoster(this);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }
        public void RegenerateInfo(bool onlyImage) {
            if (onlyImage) {
                Api.apiGetPoster(ID);
                Image.Source = new BitmapImage(new Uri(pathToImage));
            } else {
                JObject jo = new JObject();
                try {
                    jo = JObject.Parse(Api.apiGet(ShowName));
                } catch {
                    return;
                }
                Int32.TryParse(jo["data"][0]["id"].ToString(), out ID);
                Api.apiGetPoster(ID);
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path += "\\TVS-Player\\" + ID.ToString() + "\\" + ID.ToString() + ".jpg";
                Image.Source = new BitmapImage(new Uri(path));
            }
        }

    }
}
