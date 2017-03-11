using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
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
        public Library library;
        public Show selected;
        public ShowRectangle() {
            InitializeComponent();
        }

        public ShowRectangle(Show ss) {
            InitializeComponent();
            ID = ss.id;
            selected = ss;
            ShowName = ss.name;
            if (ss.posterFilename != null) {
                filename = ss.posterFilename;
            } else {
                filename = ss.id.ToString() + ".jpg";
            }
            RegenerateInfo(true);
        }

        private void ShowClicked_Event(object sender, MouseButtonEventArgs e) {
            Page showPage = new Seasons(selected);
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
            if (Disabled) {
                Storyboard sb = this.FindResource("EnableSearch") as Storyboard;
                sb.Begin();
                Disabled = false;
            }
        }

        private void ChooseImage_Event(object sender, RoutedEventArgs e) {
            Page showPage = new SelectShowPoster(this);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrameIndex(showPage);
        }
        public void RegenerateInfo(bool onlyImage) {
            if (onlyImage) {
                if (DatabaseShows.FindShow(ID).posterFilename == "own.png") {
                    String path = Helpers.path + ID + "\\own.png";
                    Image.Source = new BitmapImage(new Uri(path));
                } else if (!Api.apiGetPoster(ID, false)) {
                    String path = Helpers.path;
                    path += ID;
                    DrawText(ShowName, path, ID);
                    path += "\\own.png";
                    Image.Source = new BitmapImage(new Uri(path));
                    filename = "own.png";
                    Show s = DatabaseShows.FindShow(ID);
                    s.posterFilename = "own.png";
                    DatabaseShows.Edit(s);
                } else {
                    String path = Helpers.path + ID.ToString() + "\\" + filename;
                    try {
                        Image.Source = new BitmapImage(new Uri(path));
                    } catch {
                        filename = ID.ToString() + ".jpg";
                        path = Helpers.path + ID.ToString() + "\\" + filename;
                        Image.Source = new BitmapImage(new Uri(path));
                    }
                }
            } else {
                JObject jo = new JObject();
                try {
                    jo = JObject.Parse(Api.apiGet(ShowName));
                } catch {
                    return;
                }
                Int32.TryParse(jo["data"][0]["id"].ToString(), out ID);
                Api.apiGetPoster(ID, false);
                String path = Helpers.path + ID.ToString() + "\\" + ID.ToString() + ".jpg";
                Image.Source = new BitmapImage(new Uri(path));
            }
        }

        public static void DrawText(String text, String path, int ID) {
            StringFormat sf = new StringFormat();
            sf.Trimming = StringTrimming.Word;
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            System.Drawing.Image img = new Bitmap(680, 1000);
            Graphics drawing = Graphics.FromImage(img);
            drawing.Clear(System.Drawing.Color.FromArgb(45, 45, 45));
            drawing.DrawString(text, new Font("Segoe UI", 80f), new SolidBrush(System.Drawing.Color.White), new RectangleF(0, 0, 680, 1000), sf);
            drawing.Save();
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            try {
                img.Save(path + "\\own.png", ImageFormat.Png);
            } catch {
                MessageBox.Show("Error while creating placeholder image!");
            }
        }

        private void RemoveShow_Event(object sender, RoutedEventArgs e) {
            DatabaseShows.RemoveShowFromDb(ID);
            library.RemoveRectangle(this);
        }
    }
}
