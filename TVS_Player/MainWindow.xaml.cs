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
using TVS_Player_Base;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using TVS_Player.Properties;

namespace TVS_Player {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        private void HandPointer(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Hand;

        private void NormalPointer(object sender, MouseEventArgs e) => Mouse.OverrideCursor = null;

        private void BackButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => View.GoBack();


        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            //ResetData();
            //ForceData();
            await HandleDefaultView();
            CreateSearchEvent();
        }

        private void ForceData() {
            Settings.Default.ServerIp = "100.64.141.163";
            Settings.Default.ServerPort = 5850;
            Settings.Default.AuthToken = "";
            Settings.Default.Save();
        }

        private void ResetData() {
            Settings.Default.AuthToken = Settings.Default.ServerIp = "";
            Settings.Default.ServerPort = 0;
            Settings.Default.Save();
        }

        private void CreateSearchEvent() {
            SearchBarText.TextChanged += async (s, ev) => {
                var count = 0;
                if (SearchBarText.Text.Length > 1) {
                    var results = await Api.Search(SearchBarText.Text);
                    View.RenderSearchResult(results);
                    count = results.Count;
                }
                View.AnimateSearchResult(count);
            };

        }

        private async Task HandleDefaultView() {
            View.SetPageCustomization(View._defaultCustomization);
            if (string.IsNullOrEmpty(Settings.Default.ServerIp) && Settings.Default.ServerPort == default || !await Api.Connect(Settings.Default.ServerIp, Settings.Default.ServerPort)) {
                View.AddPage(new ServerSelector());
                MainContent.Content = new Login();
                View.CurrentPage = typeof(Login);
            } else if (string.IsNullOrEmpty(Settings.Default.AuthToken)) {
                MainContent.Content = new Login();
                View.CurrentPage = typeof(Login);
            } else {
                Api.Login(Settings.Default.AuthToken);
                MainContent.Content = new Library();
                View.CurrentPage = typeof(Library);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        }

        private void HomeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (View.CurrentPage != typeof(Library)) {
                View.SetPage(new Library());
            }
        }

        private void CalendarButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void DownloadButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }
    }
}
