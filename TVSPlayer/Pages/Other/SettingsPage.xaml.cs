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

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization pg = new PageCustomization();
            pg.MainTitle = "Settings";
            MainWindow.SetPageCustomization(pg);
            RenderInfo();
        }

        private void RenderInfo() {
            LibraryLocation.Text = Settings.Library;
            EnableDownload.IsChecked = Settings.AutoDownload;
            CacheFolder.Text = Settings.DownloadCacheLocation;
            Dir1Box.Text = Settings.FirstScanLocation;
            Dir2Box.Text = Settings.SecondScanLocation;
            Dir3Box.Text = Settings.ThirdScanLocation;
            DownSpeed.Text = Settings.DownloadSpeed.ToString();
            UpSpeed.Text = Settings.UploadSpeed.ToString();
            DownQuality.SelectedIndex = (int)Settings.DownloadQuality;
            StreamQuality.SelectedIndex = (int)Settings.StreamQuality;
            UseBuildIn.IsChecked = Settings.UseWinDefaultPlayer;
        }

        private void Dor3Select_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                Dir3Box.Text = fbd.SelectedPath;
            }
        }

        private void Dir2Select_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                Dir2Box.Text = fbd.SelectedPath;
            }
        }

        private void Dir1Select_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                Dir1Box.Text = fbd.SelectedPath;
            }
        }

        private void CacheSelect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                CacheFolder.Text = fbd.SelectedPath;
            }
        }

        private void CacheFolder_TextChanged(object sender, TextChangedEventArgs e) {
            if (DoChecks()) {
                Settings.DownloadCacheLocation = CacheFolder.Text;
            }
        }

        private void Dir1Box_TextChanged(object sender, TextChangedEventArgs e) {
            if (DoChecks()) {
                Settings.FirstScanLocation = Dir1Box.Text;
            }
        }

        private void Dir2Box_TextChanged(object sender, TextChangedEventArgs e) {
            if (DoChecks()) {
                Settings.SecondScanLocation = Dir2Box.Text;
            }
        }

        private void Dir3Box_TextChanged(object sender, TextChangedEventArgs e) {
            if (DoChecks()) {
                Settings.ThirdScanLocation = Dir3Box.Text;
            }
        }

        private async void DownSpeed_TextChanged(object sender, TextChangedEventArgs e) {
            if (!String.IsNullOrEmpty(DownSpeed.Text)) {
                if  (Int32.TryParse(DownSpeed.Text, out int result)) {
                    string oldString = DownSpeed.Text;
                    await Task.Delay(1000);
                    if (oldString == DownSpeed.Text) {
                        TorrentDownloader.SetDownloadSpeedLimit(result);
                    }
                } else {
                    await MessageBox.Show("Input is not number");
                }
            }
        }

        private async void UpSpeed_TextChanged(object sender, TextChangedEventArgs e) {
            if (!String.IsNullOrEmpty(UpSpeed.Text)) {
                if (Int32.TryParse(UpSpeed.Text, out int result)) {
                    string oldString = UpSpeed.Text;
                    await Task.Delay(1000);
                    if (oldString == UpSpeed.Text) {
                        TorrentDownloader.SetUploadSpeedLimit(result);
                    }
                } else {
                    await MessageBox.Show("Input is not number");
                }
            }
        }

        private void DownQuality_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Settings.DownloadQuality = (TorrentQuality)DownQuality.SelectedIndex;
        }

        private void StreamQuality_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Settings.StreamQuality = (TorrentQuality)StreamQuality.SelectedIndex;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            Settings.AutoDownload = (bool)EnableDownload.IsChecked;
        }

        private void Dir1Select_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Dir1Select_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private bool DoChecks() {
            if (this.IsLoaded) { 
                if (Dir1Box.Text == Dir2Box.Text && Dir1Box.Text != "Select directory") { return false; }
                if (Dir1Box.Text == Dir3Box.Text && Dir3Box.Text != "Select directory") { return false; }
                if (Dir2Box.Text == Dir3Box.Text && Dir2Box.Text != "Select directory") { return false; }
                if (Dir1Box.Text != "Select directory" && !String.IsNullOrEmpty(Dir1Box.Text)) {
                    if (!Directory.Exists(Dir1Box.Text)) { return false; }
                }
                if (Dir2Box.Text != "Select directory" && !String.IsNullOrEmpty(Dir2Box.Text)) {
                    if (!Directory.Exists(Dir2Box.Text)) { return false; }
                }
                if (Dir3Box.Text != "Select directory" && !String.IsNullOrEmpty(Dir3Box.Text)) {
                    if (!Directory.Exists(Dir3Box.Text)) { return false; }
                }
                if (CacheFolder.Text != "Select directory" && !String.IsNullOrEmpty(CacheFolder.Text)) {
                    if ((CacheFolder.Text != Dir1Box.Text && CacheFolder.Text != Dir2Box.Text && CacheFolder.Text != Dir3Box.Text)) {
                        if (!Directory.Exists(CacheFolder.Text)) { return false; }
                    }

                }
                return true;
            }
            return false;
        }

        private void UseBuildIn_Click(object sender, RoutedEventArgs e) {
            Settings.UseWinDefaultPlayer = (bool)UseBuildIn.IsChecked;
        }
    }
}
