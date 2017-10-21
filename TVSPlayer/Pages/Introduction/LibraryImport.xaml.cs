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
using TVS.API;
using Path = System.IO.Path;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Threading;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for LibraryImport.xaml
    /// </summary>
    public partial class LibraryImport : Page
    {
        public LibraryImport()
        {
            InitializeComponent();
        }
        bool gotEvents = false;
        string library;
        private void SelectFolder_GotFocus(object sender, RoutedEventArgs e) {
            SetEvents();
        }

        private void SetEvents() {
            SelectFolderText.Text = "";
            SelectFolderText.TextAlignment = TextAlignment.Left;
            SelectFolderText.GotFocus -= SelectFolder_GotFocus;
            SelectFolderText.TextChanged += SelectFolderText_TextChanged;
            gotEvents = true;
        }

        private void SelectFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                if (!gotEvents) {
                    SetEvents();
                }
                SelectFolderText.Text = fbd.SelectedPath;
            }
        }

        private void SelectFolderText_TextChanged(object sender, TextChangedEventArgs e) {
            ScanAndRenderDirs();
        }
        Thread thread;
        private void ScanAndRenderDirs() {
            if (thread != null) {
                thread.Abort();
            }
            string path = SelectFolderText.Text;
            Action a = () => ScanAndRenderDirsBackgroundTask(path);
            panel.Children.Clear();
            thread = new Thread(a.Invoke);
            thread.Start();
        }


        /// <summary>
        /// Searches for Series by directory name and renders the result
        /// </summary>
        /// <param name="path">Which directory to scan</param>
        private void ScanAndRenderDirsBackgroundTask(string path) {
            if (Directory.Exists(path)) {
                try {
                    library = path;
                    List<string> directories = Directory.GetDirectories(path).ToList();
                    Task.Delay(250);
                    foreach (string directory in directories) {
                        string name = Path.GetFileName(directory);
                        Series s = Series.SearchSingle(name);
                        if (s != null) {
                            Dispatcher.Invoke(new Action(() => {
                                Storyboard sb = (Storyboard)FindResource("OpacityUp");
                                SeriesWithFolder swf = new SeriesWithFolder(s.id);
                                swf.Height = 65;
                                swf.SeriesName.Text = s.seriesName;
                                swf.Opacity = 0;
                                swf.Remove.MouseUp += (sen, ev) => RemoveResult(swf);
                                swf.FolderLocation.Text = directory;
                                panel.Children.Add(swf);
                                sb.Begin(swf);
                            }), DispatcherPriority.Send);
                        }
                    }
                } catch (ThreadAbortException) { }
                Dispatcher.Invoke(new Action(() => {
                    if (panel.Children.Count > 0) {
                        ShowBar();
                    } else {
                        HideBar();
                    }
                }), DispatcherPriority.Send);
            }
             else {
                Dispatcher.Invoke(new Action(() => {
                    HideBar();
                }), DispatcherPriority.Send);
            }

        }

        private void RemoveResult(SeriesWithFolder swf) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard temp = sb.Clone();
            temp.Completed += (s,e) => panel.Children.Remove(swf);
            temp.Begin(swf);
        }

        private void ShowBar() {
            Storyboard sb = (Storyboard)FindResource("BarUp");
            sb.Begin(BottomBar);
        }

        private void HideBar() {
            Storyboard sb = (Storyboard)FindResource("BarDown");
            sb.Begin(BottomBar);
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.AddPage(new StartUp());
        }

        private async void AddSeries_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                if (Path.GetDirectoryName(fbd.SelectedPath) == library) {
                    Window main = Window.GetWindow(this);
                    Series show = await ((MainWindow)main).SearchShowAsync();
                    if (show.seriesName != null) {
                        Storyboard sb = (Storyboard)FindResource("OpacityUp");
                        SeriesWithFolder swf = new SeriesWithFolder(show.id);
                        swf.Height = 65;
                        swf.SeriesName.Text = show.seriesName;
                        swf.Opacity = 0;
                        swf.Remove.MouseUp += (sen, ev) => { panel.Children.Remove(swf); };
                        swf.FolderLocation.Text = fbd.SelectedPath;
                        panel.Children.Add(swf);
                        sb.Begin(swf);
                    }
                } else {
                    MessageBox.Show("Directory is not in previously selected path");
                }
            }
        }

        private async void Confirm_MouseUp(object sender, MouseButtonEventArgs e) {
            List<Tuple<int, string>> ids = new List<Tuple<int, string>>();
            foreach (var element in panel.Children) {
                SeriesWithFolder swf = (SeriesWithFolder)element;
                ids.Add(new Tuple<int, string>(swf.id,swf.FolderLocation.Text));                
            }
            Settings.Library = SelectFolderText.Text;
            await MainWindow.CreateDatabase(ids);
            List<Series> all = Database.GetSeries();

            MainWindow.RemovePage();
        }
       
        
    }
}
