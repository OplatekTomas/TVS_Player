using Ookii.Dialogs.Wpf;
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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for DbLocation.xaml
    /// </summary>
    public partial class DbLocation : Page {
        string next;
        public DbLocation(string nxt) {
            InitializeComponent();
            next = nxt;
        }
        string dbLoc;
        private void newDbLoc_TextChanged(object sender, TextChangedEventArgs e) {
            dbLoc = newDbLoc.Text;
        }
 
        private void newDbLoc_GotFocus(object sender, RoutedEventArgs e) {
            newDbLoc.Text = string.Empty;
            newDbLoc.GotFocus -= newDbLoc_GotFocus;
        }
 
        private void newDbLoc_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            var check = fbd.ShowDialog();
            if (check == true) {
                dbLoc = fbd.SelectedPath;
                newDbLoc.Text = dbLoc;
           }
       }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(dbLoc)) {
                if (next == "nothing") {
                    SettingsDB s = new SettingsDB(dbLoc);
                    AppSettings.SaveDB(s);
                    Window main = Window.GetWindow(this);
                    ((MainWindow)main).CloseTempFrame();
                    Page showPage = new ScanLocation(false);
                    ((MainWindow)main).AddTempFrame(showPage);
                } else if(next == "import") {
                    SettingsDB s = new SettingsDB(dbLoc);
                    AppSettings.SaveDB(s);
                    Window main = Window.GetWindow(this);
                    ((MainWindow)main).CloseTempFrame();
                    Page showPage = new ManageShowList(dbLoc);
                    ((MainWindow)main).AddTempFrame(showPage);
                }
            } else { MessageBox.Show("Path "+ dbLoc + " doesn't exist!","Error!"); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
    }
}
