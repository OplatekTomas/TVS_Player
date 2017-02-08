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
using System.Windows.Media.Animation;
using System.IO;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Api.getToken();
            DatabaseAPI.readDb();
        }

        private void MenuButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ShowHideMenu("sbShowLeftMenu", Sidebar);
            MenuBackground.Visibility = Visibility.Visible;
            ShowHideMenu("sbShowBackground", MenuBackground);

        }

        private void MenuHideButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
        }

        private void HideMenu() {
            ShowHideMenu("sbHideLeftMenu", Sidebar);
            MenuBackground.Visibility = Visibility.Hidden;
            ShowHideMenu("sbHideBackground", MenuBackground);
        }

        private void ShowHideMenu(string Storyboard, Grid pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);
        }
        /* private void btnShowsShow_Click(object sender, RoutedEventArgs e) {

         }
         private void btnDownloadShow_Click(object sender, RoutedEventArgs e){
          
         }*/

        private void FrameLoaded_Handler(object sender, RoutedEventArgs e) {
            Api.getToken();
        }

        public void SetFrameView(Page page) {
            if (Frame.Content.GetHashCode() != page.GetHashCode()) {
                Frame.Content = page;
            }
        }
        public void AddTempFrame(Page page) {
            Frame fr = new Frame();
            BaseGrid.Children.Add(fr);
            Panel.SetZIndex(fr, 1000);
            fr.Content = page;
        }
        public void AddTempFrameIndex(Page page) {
            Frame fr = new Frame();
            GridOnTop.Children.Add(fr);
            Panel.SetZIndex(fr, 1000);
            fr.Content = page;
        }

        public void CloseTempFrame() {
            BaseGrid.Children.RemoveAt(BaseGrid.Children.Count - 1);
        }
        public void CloseTempFrameIndex() {
            GridOnTop.Children.RemoveAt(GridOnTop.Children.Count - 1);
        }

        private void SearchBar_GotFocus(object sender, RoutedEventArgs e) {
            SearchBar.Text = "";
            SearchBar.GotFocus -= SearchBar_GotFocus;
        }

        private void ShowLibraryButon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Shows)) {
                Frame.Content = new Shows();
            }
        }

        private void DownloadPageButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Download)) {
                Api.getToken();
                //Api.apiGetPoster(73871,"Futurama");
                //string kappa = Api.apiGet(1,1, 73871);
                //Frame.Content = new Startup();

                Checker.UpdateShowFull(277928);
                //Checker.CheckForUpdates(121361);
            }
        }

        public void SetHeading(string text) {
            HeadingText.Text = text;
        }

        private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void InfoButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }
    }
}
