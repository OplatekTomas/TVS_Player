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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

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
        List<Shows> shows = new List<Shows>();
        public struct Shows {
            public string id;
            public string name;
            public Shows(string id, string name) : this() {
                this.id = id;
                this.name = name;
            }

        }
        private void readFolders() {
            subfolders = Directory.GetDirectories(location).ToList<string>();
            Action list;
            list = () => listFolders();
            Thread thread = new Thread(list.Invoke);
            thread.Name = "List shows";
            thread.Start();

        }
        private void addUI(string show, string folder) {
            JObject jo = JObject.Parse(show);
            DBScanOption option = new DBScanOption();
            string name = jo["data"][0]["seriesName"].ToString();
            string id = jo["data"][0]["id"].ToString();
            option.showName.Text = name;
            option.showLocation.Text = folder;
            shows.Add(new Shows(id, name));
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
            for (int i = 0; i < shows.Count(); i++){
                DatabaseAPI.addShowToDb(shows[i].id, shows[i].name, true);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
    }
}
