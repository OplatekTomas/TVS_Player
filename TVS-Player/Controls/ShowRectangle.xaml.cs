using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
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
        public int ID = -1;
        public string ShowName = "NON";
        public bool Disabled = false;
        public string filename;
        public Shows library;
        public ShowRectangle() {
            InitializeComponent();
        }

        public ShowRectangle(SelectedShows ss) {
            InitializeComponent();
            ID = Int32.Parse(ss.idSel);
            ShowName = ss.nameSel;
            if (ss.posterFilename != null) {
                filename = ss.posterFilename;
            } else {
                filename = ID.ToString() + ".jpg";
            }
            RegenerateInfo(true);
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
                Api.apiGetPoster(ID,false);
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path += "\\TVS-Player\\" + ID.ToString() + "\\" + filename;
                try {
                    Image.Source = new BitmapImage(new Uri(path));
                } catch {
                    filename = ID.ToString() + ".jpg";
                    path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    path += "\\TVS-Player\\" + ID.ToString() + "\\" + filename;
                    Image.Source = new BitmapImage(new Uri(path));
                }
            } else {
                JObject jo = new JObject();
                try {
                    jo = JObject.Parse(Api.apiGet(ShowName));
                } catch {
                    return;
                }
                Int32.TryParse(jo["data"][0]["id"].ToString(), out ID);
                Api.apiGetPoster(ID,false);
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path += "\\TVS-Player\\" + ID.ToString() + "\\" + ID.ToString() + ".jpg";
                Image.Source = new BitmapImage(new Uri(path));
            }
            String paths = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            paths += "\\TVS-Player\\" + ID.ToString() + "\\own.png";
            DrawText(ShowName,paths);
            Image.Source = new BitmapImage(new Uri(paths));
        }

        public static void DrawText(String text,String path) {
            StringFormat sf = new StringFormat();
            sf.Trimming = StringTrimming.Word;
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            System.Drawing.Image img = new Bitmap(680,1000);
            Graphics drawing = Graphics.FromImage(img);
            drawing.Clear(System.Drawing.Color.FromArgb(45,45,45));
            drawing.DrawString(text, new Font("Segoe UI", 80f),new SolidBrush(System.Drawing.Color.White),new RectangleF(0, 0,680,1000), sf);
            drawing.Save();
            try {
                img.Save(path, ImageFormat.Png);
            } catch { }
        }

        private void RemoveShow_Event(object sender, RoutedEventArgs e) {
            DatabaseAPI.removeShowFromDb(ID.ToString(), true);
            library.RemoveRectangle(this);
        }
    }
}
