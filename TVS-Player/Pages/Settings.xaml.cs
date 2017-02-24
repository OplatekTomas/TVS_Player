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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page {
        public Settings() {
            InitializeComponent();
            Window m = Application.Current.MainWindow;
            ((MainWindow)m).SetTitle("Settings");
            LoadSettings();
        }

        private void LoadSettings() {
            SettingsDB s =  AppSettings.ReadDB();
            EPPlayer.IsChecked = !s.BuildInPlayer;
        }

        private void EPPlayer_Click(object sender, RoutedEventArgs e) {
            if (EPPlayer.IsChecked == true) {
                AppSettings.SetBuildInPlayer(false);
            } else {
                AppSettings.SetBuildInPlayer(true);
            }
        }
    }
}
