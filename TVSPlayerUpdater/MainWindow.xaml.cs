using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
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
using System.Reflection;
using System.Windows.Shapes;
using System.Security.Permissions;
using System.Net;
using Newtonsoft.Json.Linq;

namespace TVSPlayerUpdater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            if (!CanWrite()) {
                RunAsAdmin();
            } else {
                Update();
            }

        }

        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        private void RunAsAdmin() {
            Update();
        }


        private void Update() {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0");
            wc.Headers.Add("Accept", "application/vnd.github.v3+json");
            var response = wc.DownloadString("https://api.github.com/repos/Kaharonus/TVS-Player/releases");
            JArray jo = JArray.Parse(response);
            var serverTime = DateTime.Parse(wc.ResponseHeaders["Date"]);
            var releaseTime = DateTime.Parse(jo[0]["published_at"].ToString());
            if (DateTime.Parse(jo[0]["published_at"].ToString()) > DateTime.Parse(wc.ResponseHeaders["Date"])) {

            }
        }

        private bool CanWrite() {
            try {
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(Assembly.GetExecutingAssembly().Location);
                return true;
            } catch (UnauthorizedAccessException) {
                return false;
            }
        }

    }
}
