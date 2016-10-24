using System;
using System.Windows;
using System.Windows.Controls;
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
            Page showPage = new ShowList();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }

        private void importDB_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Json files (*.json)|*.json";
            ofd.Multiselect = false;
            string path;
            string moveTo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            var check = ofd.ShowDialog();
            if (check == DialogResult.OK) {
                path = ofd.FileName;
                if (File.Exists(moveTo)) {
                    File.Delete(moveTo);
                }
                File.Move(path,moveTo);
            }
        }
    }
}
