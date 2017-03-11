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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Page {
        public About() {
            InitializeComponent();
            Window m = Application.Current.MainWindow;
            ((MainWindow)m).SetTitle("About");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void Subs_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/AlexPoint/SubtitlesParser");
        }

        private void tvdb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("www.thetvdb.com");
        }

        private void petko_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/TheNumerus/");
        }

        private void icons_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            List<string> links = new List<string>() { "http://www.flaticon.com/authors/eleonor-wang", "http://www.flaticon.com/authors/balraj-chana", "http://www.flaticon.com/authors/madebyoliver", "http://www.flaticon.com/authors/google", "http://www.flaticon.com/authors/iconnice", "http://www.flaticon.com/authors/fps-web-agency", "http://www.flaticon.com/authors/freepik", "http://www.flaticon.com/authors/gregor-cresnar", "http://www.flaticon.com/authors/amit-jakhu" };
            foreach (string url in links) {
                System.Diagnostics.Process.Start(url);
            }
        }

        private void flaticon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("www.flaticon.com");
        }

        private void freepik_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("http://www.flaticon.com/authors/freepik");
        }

        private void oliver_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("http://www.flaticon.com/authors/madebyoliver");
        }
    }
}
