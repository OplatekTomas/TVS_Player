using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

        public struct Shows {
            string id;
            string name;
        }

        private void readFolders() {
            List < string > subfolders = new List<String>();
             subfolders = Directory.GetDirectories(location).ToList<string>();
             foreach (string folder in subfolders) {
             string show = Api.apiGet(Path.GetDirectoryName(folder));
                 if (show != null) {
                    JObject jo = JObject.Parse(show);
                 }

             }

        }

        private void editShow_Click(object sender, RoutedEventArgs e) {

        }

        private void removeShow_Click(object sender, RoutedEventArgs e) {

        }

        private void showInfo_Click(object sender, RoutedEventArgs e) {

        }
    }
}
