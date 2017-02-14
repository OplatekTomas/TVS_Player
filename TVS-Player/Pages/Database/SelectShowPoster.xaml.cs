using Newtonsoft.Json.Linq;
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
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for SelectShowPoster.xaml
    /// </summary>
    public partial class SelectShowPoster : Page {
        private ShowRectangle sr;
        public PosterSelector selected;
        public SelectShowPoster() {
            InitializeComponent();
        }
        public SelectShowPoster(ShowRectangle sri) {
            sr = sri;
            InitializeComponent();
            Action downloadImages;
            downloadImages = () => downloadAll(sr.ID);
            Thread t = new Thread(downloadImages.Invoke);
            t.Start();
        }
        private void downloadAll(int id) {
            JObject jo = JObject.Parse(Api.apiGetAllPosters(sr.ID));
            for (int i = 0; i < jo["data"].Count() - 1; i++) {
                string filename = jo["data"][i]["thumbnail"].ToString();
                int index = Int32.Parse(filename.Substring(filename.IndexOf("-") + 1, filename.IndexOf(".") - filename.IndexOf("-") - 1));
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (i == 0) {
                    Api.apiGetPoster(sr.ID, true);
                    path += "\\TVS-Player\\" + sr.ID.ToString() + "\\Thumbnails\\" + sr.ID.ToString() + ".jpg";
                } else {
                    Api.apiGetPoster(sr.ID, sr.ID + "-" + index + ".jpg", i, true);
                    path += "\\TVS-Player\\" + sr.ID.ToString() + "\\Thumbnails\\" + sr.ID.ToString() + "-" + index + ".jpg";
                }
                Dispatcher.Invoke(new Action(() => {
                    PosterSelector ps = new PosterSelector(path, i, this);
                    posterList.Children.Add(ps);
                }), DispatcherPriority.Send);
            }
        }

        private void CancelButton_Event(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }

        private void SelectButton_Event(object sender, RoutedEventArgs e) {
            if (selected != null) {
                string filename = Path.GetFileName(selected.path);
                Api.apiGetPoster(sr.ID,filename,0, false);
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sr.filename = filename;
                sr.RegenerateInfo(true);
                Show s = DatabaseShows.FindShow(sr.ID);
                s.posterFilename = filename;
                DatabaseShows.Edit(s);
                Window main = Window.GetWindow(this);
                ((MainWindow)main).CloseTempFrame();
            }
        }
    }
}
