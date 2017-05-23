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
using System.Diagnostics;
using System.Globalization;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for ImportScanFolder.xaml
    /// </summary>
    public partial class ImportScanFolder : Page {
        public ImportScanFolder() {
            InitializeComponent();
        }

        //Starts animation parameters are name of storyboard that will animate and grid that will animate
        private void StartAnimation( string storyboard , Grid pnl ) {
            Storyboard sb = this.FindResource(storyboard) as Storyboard;
            sb.Begin(pnl);
        }

        //Sets UI elements away from screen and animates them returning
        private void Page_Loaded(object sender, RoutedEventArgs e) {
            TranslateTransform t = new TranslateTransform(0, BackGrid.ActualHeight);
            BGrid.MouseUp += Grid_MouseUp;
            Content.RenderTransform = t;
            StartAnimation("SlideToMiddle", Content);
            StartAnimation("OpacityUp", BackGrid);
        }

        //Closes this page
        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
        }

        //Removes default text from bar with location
        private void LocationBox_GotFocus(object sender, RoutedEventArgs e) {
            LocationBox.GotFocus -= LocationBox_GotFocus;
            LocationBox.Text = "";
            LocationBox.TextChanged += (s, ea) => ScanStart();
        }

        //Starts scanning as a new thread
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
        //Actual scan in a new thread
        private void Scan(string text) {
            if (Directory.Exists(text)){
                List<string> folders = Directory.GetDirectories(text).ToList();
                foreach (string folder in folders) {
                    TVShow show = TVShow.SearchSingle(Path.GetFileName(folder));
                    if (show.seriesName != null) {
                    Dispatcher.Invoke(new Action(() => {
                        ScannedShow sh = new ScannedShow(show);
                        sh.Height = 60;
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

        //Removes show from a list of foumd shows
        private void RemoveShow(ScannedShow sh) {
            ShowList.Children.Remove(sh);
        }

        //Enables you to change what show has been found (if wrong show has been found)
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

        //shows you details about specific show (At least show name and show id from TVMaze are requiered)
        private void ShowDetails(TVShow show) {
            MainWindow.AddPage(new Details(show));
        }

        //Enables you to select folder instead of manualy writing it in the bar with location
        private void OpenFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != "") {
                LocationBox.GotFocus -= LocationBox_GotFocus;
                LocationBox.TextChanged += (s, ea) => ScanStart();
                LocationBox.Text = fbd.SelectedPath;
            }
        }

        //Closes this page
        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
        }

        //Adds show to the list
        private async void AddShow_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != "") {
                if (!AlreadyExists(fbd.SelectedPath)) {
                    Window main = Window.GetWindow(this);
                    TVShow show = await ((MainWindow)main).SearchShowAsync();
                    if (show != null) {
                        ScannedShow sh = new ScannedShow(show);
                        sh.Height = 60;
                        sh.ShowName.Text = show.seriesName;
                        sh.FolderName.Text = fbd.SelectedPath;
                        sh.RemoveIcon.MouseUp += (se, ea) => RemoveShow(sh);
                        sh.EditIcon.MouseUp += (se, ea) => EditShow(sh, show);
                        sh.DetailsIcon.MouseUp += (se, ea) => ShowDetails(show);
                        ShowList.Children.Add(sh);
                    }
                }
            }
        }

        //Checks if folder is already in ShowList - return true if it already is in there
        private bool AlreadyExists(string folder) {
            foreach (UIElement ue in ShowList.Children) {
                ScannedShow sh = ue as ScannedShow;
                if (sh.FolderName.Text == folder) {
                    return true;
                }
            }
            return false;
        }

        private void Next_MouseUp(object sender, MouseButtonEventArgs e) {
            HashSet<TVShow> list = new HashSet<TVShow>();
            foreach (UIElement ue in ShowList.Children) {
                ScannedShow sh = ue as ScannedShow;
                TVShow t = sh.show;
                t.GetInfo();
                list.Add(t);
            }          
            Database.SaveTVShows(list.ToList());
            int test = 0;
            foreach (TVShow s in list) {
                Action a = () => { 
                    List<Episode> lst = Episode.getAllEP(s);
                    Database.SaveEpisodes(s, lst);
                };
                Task t = new Task(a.Invoke);
                t.Start();
                t.ContinueWith((t2) => test++);
            }
            while (test != list.Count) { Thread.Sleep(50); }
        }
    }
}
