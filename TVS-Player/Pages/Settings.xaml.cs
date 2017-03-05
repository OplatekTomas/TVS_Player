using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
            Window m = System.Windows.Application.Current.MainWindow;
            ((MainWindow)m).SetTitle("Settings");
            LoadSettings();
        }

        private void LoadSettings() {
            SettingsDB s =  AppSettings.ReadDB();
            EPPlayer.IsChecked = !s.BuildInPlayer;
            AutoDownload.IsChecked = s.AutoDownload;
            OneClickDown.IsChecked = s.OneClickDownload;
            OneClickQuality.SelectedValue = s.OneClickQuality;
            AutoQuality.SelectedValue = s.AutoQuality;
            if (!s.AutoDownload) {
                AutoQuality.IsEnabled = false;
            }
            if (!s.OneClickDownload) {
                AutoQuality.IsEnabled = false;
            }
            List<string> scanpaths = AppSettings.GetLocations();
            foreach (string p in scanpaths) {
                FolderControl fc = new FolderControl();
                fc.pathBox.Text = p;
                fc.editLocation.Click += (se,e) => editOption(fc);
                fc.removeLocation.Click += (se, e) => removeOption(fc);
                ScanList.Children.Add(fc);
            }
        }

        private void EPPlayer_Click(object sender, RoutedEventArgs e) {
            if (EPPlayer.IsChecked == true) {
                AppSettings.SetBuildInPlayer(false);
            } else {
                AppSettings.SetBuildInPlayer(true);
            }
        }

        private void AddLoc_MouseUp(object sender, MouseButtonEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var res = fbd.ShowDialog();
            if (res == DialogResult.OK) {
                FolderControl fc = new FolderControl();
                fc.pathBox.Text = fbd.SelectedPath;
                fc.editLocation.Click += (se, ea) => editOption(fc);
                fc.removeLocation.Click += (se, ea) => removeOption(fc);
                ScanList.Children.Add(fc);
                AppSettings.AddLocation(fbd.SelectedPath);
            }
        }
        private void removeOption(FolderControl fc) {
            ScanList.Children.Remove(fc);
            AppSettings.RemoveLocation(fc.pathBox.Text);
        }
        private void editOption(FolderControl fc) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var res = fbd.ShowDialog();
            if (res == DialogResult.OK) {
                AppSettings.EditLocation(fc.pathBox.Text,fbd.SelectedPath);
                fc.pathBox.Text = fbd.SelectedPath;
            }

        }

        private void AutoDownload_Click(object sender, RoutedEventArgs e) {
            if (AutoDownload.IsChecked == true) {
                AppSettings.SetAutoDownload(true);
                AutoQuality.IsEnabled = true;
            } else {
                AppSettings.SetAutoDownload(false);
                AutoQuality.IsEnabled = false;
            }
        }

        private void OneClickDown_Click(object sender, RoutedEventArgs e) {
            if (OneClickDown.IsChecked == true) {
                AppSettings.SetOneClick(true);
                OneClickQuality.IsEnabled = true;
            } else {
                AppSettings.SetOneClick(false);
                OneClickQuality.IsEnabled = false;
            }
        }

        private void AutoQuality_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (AutoQuality.SelectedValue != null) { 
                AppSettings.SetAutoQuality(AutoQuality.SelectedValue.ToString());
            }
        }

        private void OneClickQuality_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (OneClickQuality.SelectedValue != null) { 
                AppSettings.SetOneClickQuality(OneClickQuality.SelectedValue.ToString());
            }
        }
    }
}
