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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Threading;
using Path = System.IO.Path;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for ImportScanFolder.xaml
    /// </summary>
    public partial class ImportScanFolder : Page {
        public ImportScanFolder() {
            InitializeComponent();
        }
        private void StartAnimation( string storyboard , Grid pnl ) {
            Storyboard sb = this.FindResource(storyboard) as Storyboard;
            sb.Begin(pnl);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            TranslateTransform t = new TranslateTransform(0, BackGrid.ActualHeight);
            BGrid.MouseUp += Grid_MouseUp;
            Content.RenderTransform = t;
            StartAnimation("SlideToMiddle", Content);
            StartAnimation("OpacityUp", BackGrid);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).RemovePage();
        }

        private void LocationBox_GotFocus(object sender, RoutedEventArgs e) {
            LocationBox.GotFocus -= LocationBox_GotFocus;
            LocationBox.Text = "";
            LocationBox.TextChanged += (s, ea) => ScanStart();
        }
        Thread t;
        private void ScanStart() {
            string text = LocationBox.Text;
            ShowList.Children.Clear();
            if (t != null) {
                t.Abort();
            }
            Action a = () => Scan(text);
            t = new Thread(a.Invoke);
            t.Start();
            
        }

        private void Scan(string text) {
            if (Directory.Exists(text)){
                List<string> folders = Directory.GetDirectories(text).ToList();
                foreach (string folder in folders) {
                    TVShow show = TVShow.SearchSingle(Path.GetFileName(folder));
                    if (show.seriesName != null) {
                    Dispatcher.Invoke(new Action(() => {
                        ScannedShow sh = new ScannedShow();
                        sh.Height = 50;
                        sh.ShowName.Text = show.seriesName;
                        sh.FolderName.Text = folder;
                        sh.RemoveIcon.MouseUp += (s, e) => RemoveShow(sh);
                        sh.EditIcon.MouseUp += (s, e) => EditShow(sh, show);
                        sh.DetailsIcon.MouseUp += (s, e) => ShowDetails(show);
                        ShowList.Children.Add(sh);
                    }));
                    }
                }
            }
        }

        private void RemoveShow(ScannedShow sh) {
            ShowList.Children.Remove(sh);
        }

        private async void EditShow(ScannedShow sh,TVShow show) {
            Window main = Window.GetWindow(this);
            show = await ((MainWindow)main).SearchShowAsync();
            if (show != null) { 
                sh.ShowName.Text = show.seriesName;
                sh.RemoveIcon.MouseUp += (se, e) => RemoveShow(sh);
                sh.EditIcon.MouseUp += (se, e) => EditShow(sh, show);
                sh.DetailsIcon.MouseUp += (se, e) => ShowDetails(show);
            }
        }
        private void ShowDetails(TVShow show) {

        }

        private void OpenFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != "") {
                LocationBox.GotFocus -= LocationBox_GotFocus;
                LocationBox.TextChanged += (s, ea) => ScanStart();
                LocationBox.Text = fbd.SelectedPath;
            }
        }

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).RemovePage();
        }
    }
}
