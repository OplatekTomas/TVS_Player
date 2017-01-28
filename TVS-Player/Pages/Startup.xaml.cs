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
            /*Page showPage = new ShowList("createdb");
            //Page showPage = new ScanLocation();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);*/
            
            
            //Renamer.RenameBatch(new List<int>(){ 121361 },new List<string>() {"D:\\Test\\S1" }, "D:\\Test\\DB");
            //DatabaseEpisodes.readDb(121361);
            //int k = DatabaseAPI.GetSeasons(121361);
        }

        private void importDB_Click(object sender, RoutedEventArgs e) {
            Page showPage = new DbLocation("import");
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }


    }
}
