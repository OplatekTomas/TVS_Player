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
            
            JObject jo = JObject.Parse(Api.apiGetAllPosters(sr.ID));
            for(int i = 0; i < jo["data"].Count()-1; i++) {
                String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (i == 0) {
                    Api.apiGetPoster(sr.ID);
                    path += "\\TVS-Player\\" + sr.ID.ToString() + "\\" + sr.ID.ToString() + ".jpg";
                } else {
                    Api.apiGetPoster(sr.ID,jo["data"][i]["thumbnail"].ToString(),i,true);
                    path += "\\TVS-Player\\" + sr.ID.ToString() + "\\" + sr.ID.ToString() + "-" + i + ".jpg";
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
                Api.apiGetPoster(sr.ID, "posters/" + filename,0, false);
                sr.pathToImage = selected.path;
                sr.RegenerateInfo(true);
                Window main = Window.GetWindow(this);
                ((MainWindow)main).CloseTempFrame();
            }
        }
    }
}
