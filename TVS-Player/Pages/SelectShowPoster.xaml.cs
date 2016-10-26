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
            //MessageBox.Show(Api.apiGetAllPosters(sr.ID));
            JObject jo = JObject.Parse(Api.apiGetAllPosters(sr.ID));
            for(int i = 0; i < jo["data"].Count()-1; i++) {
                string filename = jo["data"][i]["thumbnail"].ToString();
                int index = Int32.Parse(filename.Substring(filename.IndexOf("-")+1, filename.IndexOf(".") - filename.IndexOf("-")-1));
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (i == 0) {
                    Api.apiGetPoster(sr.ID,true);
                    path += "\\TVS-Player\\" + sr.ID.ToString() + "\\Thumbnails\\" + sr.ID.ToString() + ".jpg";
                } else {
                    Api.apiGetPoster(sr.ID,sr.ID+"-"+index+".jpg",i,true);
                    path += "\\TVS-Player\\" + sr.ID.ToString() + "\\Thumbnails\\" + sr.ID.ToString() + "-" + index + ".jpg";
                }
                PosterSelector ps = new PosterSelector(path,i,this);
                posterList.Children.Add(ps);
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
                DatabaseAPI.FindShowByID(sr.ID.ToString()).posterFilename = filename;
                DatabaseAPI.saveDB();
                Window main = Window.GetWindow(this);
                ((MainWindow)main).CloseTempFrame();
            }
        }
    }
}
