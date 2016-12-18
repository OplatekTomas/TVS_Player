using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Page {
        public Startup() {
            InitializeComponent();
            Api.getToken();
        }

        private void addDB_Click(object sender, RoutedEventArgs e) {
            //Page showPage = new ShowList("createdb");
            /*Page showPage = new ScanLocation();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);*/

            //List<string> test = Renamer.FilterExtensions(Renamer.ScanEpisodes(new List<string>{"D:\\Downloads\\Torrent"}, 263365));
            //Api.GetAliases();
            //Tuple<int,int> t = Renamer.GetInfo("D:\\Game of Thrones S01E01 - píčovina.avi");
            //Renamer.DownloadInfo(121361);
            Renamer.Rename(new List<string>() { "D:\\Game of Thrones S01E01 - píčovina.avi"},"D:\\",121361,"Game of Thrones");
        }

        private void importDB_Click(object sender, RoutedEventArgs e) {
            Page showPage = new DbLocation("import");
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }


    }
}
