using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;
using System.Web;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ManageShowList.xaml
    /// </summary>
    public partial class ManageShowList : Page {
        string location;
        public ManageShowList(string loc) {
            InitializeComponent();
            location = loc;
            readFolders();
        }
        List<string> subfolders = new List<String>();
        List<Show> shows = new List<Show>();
        private void readFolders() {
            subfolders = Directory.GetDirectories(location).ToList<string>();
            Action list;
            list = () => listFolders();
            Thread thread = new Thread(list.Invoke);
            thread.Name = "List shows";
            thread.Start();

        }
        private void addUI(string json, string folder) {
            JObject jo = JObject.Parse(json);
            DBScanOption option = new DBScanOption();
            string name = jo["data"][0]["seriesName"].ToString();
            string id = jo["data"][0]["id"].ToString();
            string status = jo["data"][0]["status"].ToString();
            Show show = new Show(Int32.Parse(id),name,status);
            option.showName.Text = name;
            option.showLocation.Text = folder;
            option.showInfo.Click += (s, e) => {              
                showInfo(Api.apiGet(Int32.Parse(id))); 
            };
            option.editShow.Click += (s, e) => {
                editShow(show, option);
            };
            option.removeShow.Click += (s, e) => {
                removeShow(show, option);
            };
            shows.Add(show);
            panel.Children.Add(option);
        }

        private void listFolders() {
            foreach (string folder in subfolders) {           
                string show = Api.apiGet(Path.GetFileName(folder));
                if (show != null) {
                    Dispatcher.Invoke(new Action(() => {
                        addUI(show, folder);
                    }), DispatcherPriority.Send);
                }
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            List<int> id = new List<int>();
            List<string> locs = new List<string>();
            foreach(Show s in shows) { 
                    DatabaseShows.AddShowToDb(s);
                    id.Add(s.id);
            }
            foreach (DBScanOption fc in panel.Children) {
                locs.Add(fc.showLocation.Text);
            }
            Page page = new Library();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();

            Renamer.RenameBatch(id,locs,AppSettings.GetLibLocation());
            ((MainWindow)main).SetFrameView(page);
            ((MainWindow)main).CloseTempFrameIndex();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
        }
        private void showInfo(string specific) {
            Page showPage = new ShowInfoSmall(specific);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrameIndex(showPage);
        }
        private void removeShow(Show s, DBScanOption option) {
            int index = shows.FindIndex(sh => sh.id == s.id);
            shows.RemoveAt(index);
            panel.Children.Remove(option);
        }
        private async void editShow(Show s, DBScanOption option) {
            Page showPage = new SelectShow();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrameIndex(showPage);
            var show = await Helpers.showSelector();
            int index = shows.FindIndex(sh => sh.id == s.id);
            option.showName.Text = show.name;
            shows[index] = show;

        }

    }
}
