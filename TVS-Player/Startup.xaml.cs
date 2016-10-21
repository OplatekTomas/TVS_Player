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
using System.Windows.Forms;
using System.IO;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Page {
        public Startup() {
            InitializeComponent();
        }

        private void addDB_Click(object sender, RoutedEventArgs e) {
            AddShows addSW = new AddShows();
            addSW.Show();
        }

        private void importDB_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Formats (*.json)|.json";
            ofd.Multiselect = false;
            string path;
            string moveTo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            var check = ofd.ShowDialog();
            if (check == DialogResult.OK) {
                path = ofd.FileName;
                File.Move(path,moveTo);
            }
        }

    }
}
