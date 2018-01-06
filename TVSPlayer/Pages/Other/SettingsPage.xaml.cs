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
            PerformanceMode.IsChecked = Settings.PerformanceMode;
            CacheFolder.TextChanged += CacheFolder_TextChanged;
            Dir1Box.TextChanged += Dir1Box_TextChanged;
            Dir2Box.TextChanged += Dir2Box_TextChanged;
            Dir3Box.TextChanged += Dir3Box_TextChanged;
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

        private async void CacheFolder_TextChanged(object sender, TextChangedEventArgs e) {
            string[] strings = new string[3] { Dir1Box.Text.ToLower(), Dir2Box.Text.ToLower(), Dir2Box.Text.ToLower() };
            if ((!String.IsNullOrEmpty(CacheFolder.Text) && Directory.Exists(CacheFolder.Text) && !strings.Contains(CacheFolder.Text.ToLower())) || String.IsNullOrEmpty(CacheFolder.Text)) {
                Settings.DownloadCacheLocation = CacheFolder.Text;
            } else {
                await MessageBox.Show("Directories cannot be same");
            }
        }

        private async void Dir1Box_TextChanged(object sender, TextChangedEventArgs e) {
            string[] strings = new string[3] { CacheFolder.Text.ToLower(), Dir2Box.Text.ToLower(), Dir2Box.Text.ToLower() };
            if (!String.IsNullOrEmpty(Dir1Box.Text) && Directory.Exists(Dir1Box.Text) && !strings.Contains(Dir1Box.Text.ToLower()) || String.IsNullOrEmpty(Dir1Box.Text)) {
                Settings.FirstScanLocation = Dir1Box.Text;
            } else {
                await MessageBox.Show("Directories cannot be same");
            }
        }

        private async void Dir2Box_TextChanged(object sender, TextChangedEventArgs e) {
            string[] strings = new string[3] { Dir1Box.Text.ToLower(), CacheFolder.Text.ToLower(), Dir2Box.Text.ToLower() };
            if (!String.IsNullOrEmpty(Dir2Box.Text) && Directory.Exists(Dir2Box.Text) && !strings.Contains(Dir2Box.Text.ToLower()) || String.IsNullOrEmpty(Dir2Box.Text)) {
                Settings.SecondScanLocation = Dir2Box.Text;
            } else {
                await MessageBox.Show("Directories cannot be same");
            }
        }

        private async void Dir3Box_TextChanged(object sender, TextChangedEventArgs e) {
            string[] strings = new string[3] { Dir1Box.Text.ToLower(), Dir2Box.Text.ToLower(), CacheFolder.Text.ToLower() };
            if (!String.IsNullOrEmpty(Dir3Box.Text) && Directory.Exists(Dir3Box.Text) && strings.Contains(Dir3Box.Text.ToLower()) || String.IsNullOrEmpty(Dir3Box.Text)) {
                Settings.ThirdScanLocation = Dir3Box.Text;
            } else {
                await MessageBox.Show("Directories cannot be same");
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

       

        private void UseBuildIn_Click(object sender, RoutedEventArgs e) {
            Settings.UseWinDefaultPlayer = (bool)UseBuildIn.IsChecked;
        }

        private void PerformanceMode_Click(object sender, RoutedEventArgs e) {
            Settings.PerformanceMode = (bool)PerformanceMode.IsChecked;
        }
    }
}
