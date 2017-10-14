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
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Animation;
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for LibraryCreate.xaml
    /// </summary>
    public partial class LibraryCreate : Page {
        public LibraryCreate() {
            InitializeComponent();
        }

        private void FolderLocation_GotFocus(object sender, RoutedEventArgs e) {
            SetFolderLocation();
        }

        private void SetFolderLocation() {
            FolderLocation.GotFocus -= FolderLocation_GotFocus;
            FolderLocation.Text = "";
        }

        private void SelectFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                SetFolderLocation();
                FolderLocation.Text = fbd.SelectedPath;
            }
        }
        /// <summary>
        /// Hide this function in VS and do not look at it.. ever
        /// </summary>
        /// <returns>true if everything is fine, false if it isn't</returns>
        private bool DoChecks() {
            if (FirstScan.Text == SecondScan.Text && FirstScan.Text != "Select directory") { return false; }
            if (FirstScan.Text == ThirdScan.Text && ThirdScan.Text != "Select directory") { return false; }
            if (SecondScan.Text == ThirdScan.Text && SecondScan.Text != "Select directory") { return false; }
            if(FolderLocation.Text == FirstScan.Text || FolderLocation.Text == SecondScan.Text || FolderLocation.Text == ThirdScan.Text) { return false; }
            if (!Directory.Exists(FolderLocation.Text)) { return false; }
            if (FirstScan.Text != "Select directory" && !String.IsNullOrEmpty(FirstScan.Text)) {
                if (!Directory.Exists(FirstScan.Text)) { return false; }
            }
            if (SecondScan.Text != "Select directory" && !String.IsNullOrEmpty(SecondScan.Text)) {
                if (!Directory.Exists(SecondScan.Text)) { return false; }
            }
            if (ThirdScan.Text != "Select directory" && !String.IsNullOrEmpty(ThirdScan.Text)) {
                if (!Directory.Exists(ThirdScan.Text)) { return false; }
            }
            return true;
        }

        private void Now_MouseUp(object sender, MouseButtonEventArgs e) {
            if (!DoChecks()) {
                MessageBox.Show("Either one of the locations you entered does not exist or few of them are same");
            } else {
                SaveLocations();
                TextBoxGrid.VerticalAlignment = VerticalAlignment.Top;
                TextBoxGrid.Margin = new Thickness(10, TopRow.ActualHeight - 57, 10, 0);
                Storyboard moveUp = (Storyboard)FindResource("MoveUp");
                moveUp.Begin(TextBoxGrid);
                Storyboard fadeOut = (Storyboard)FindResource("OpacityDown");
                fadeOut.Begin(Header);
                fadeOut.Begin(Now);
                fadeOut.Begin(Later);
                fadeOut.Begin(Optionals);
                Storyboard fadeIn = (Storyboard)FindResource("OpacityUp");
                Scroll.Visibility = Visibility.Visible;
                fadeIn.Begin(Scroll);
                AddShow();
            }
        }

        private void SaveLocations() {
            Properties.Settings.Default.FirstScan = FirstScan.Text;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.SecondScan = SecondScan.Text;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.ThirdScan = ThirdScan.Text;
            Properties.Settings.Default.Save();
        }

        private async void AddShow() {
            Window main = Window.GetWindow(this);
            Series show = await ((MainWindow)main).SearchShowAsync();
            if (show.seriesName != null) {
                SeriesWithoutFolderLibCreation greatClassNameIdiot = new SeriesWithoutFolderLibCreation(show.id);
                greatClassNameIdiot.Remove.MouseUp += (s, e) => Remove(greatClassNameIdiot);
                greatClassNameIdiot.Height = 50;
                greatClassNameIdiot.Opacity = 0;
                greatClassNameIdiot.SeriesName.Text = show.seriesName;
                panel.Children.Add(greatClassNameIdiot);
                Storyboard sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(greatClassNameIdiot);
            }
        }
        

        private void Remove(SeriesWithoutFolderLibCreation result) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard temp = sb.Clone();
            temp.Completed += (s,e) => { panel.Children.Remove(result); };
            temp.Begin(result);
        }

        private void Later_MouseUp(object sender, MouseButtonEventArgs e) {

        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();           
            MainWindow.AddPage(new StartUp());
        }

        private void Confirm_MouseUp(object sender, MouseButtonEventArgs e) {

        }

        private void AddSeries_MouseUp(object sender, MouseButtonEventArgs e) {
            AddShow();
        }
        #region BasicEvents
        private void ThirdScan_GotFocus(object sender, RoutedEventArgs e) {
            ThirdScan.GotFocus -= ThirdScan_GotFocus;
            ThirdScan.Text = "";
        }

        private void ThirdFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                ThirdScan.Focus();
                ThirdScan.Text = fbd.SelectedPath;
            }
        }

        private void FirstFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                FirstScan.Focus();
                FirstScan.Text = fbd.SelectedPath;
            }
        }

        private void FirstScan_GotFocus(object sender, RoutedEventArgs e) {
            FirstScan.GotFocus -= FirstScan_GotFocus;
            FirstScan.Text = "";
        }

        private void SecondScan_GotFocus(object sender, RoutedEventArgs e) {
            SecondScan.GotFocus -= SecondScan_GotFocus;
            SecondScan.Text = "";
        }

        private void SecondFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                SecondScan.Focus();
                SecondScan.Text = fbd.SelectedPath;
            }
        }
#endregion
    }
}
