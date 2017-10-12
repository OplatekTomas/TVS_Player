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

        private void SelectFolder_GotFocus(object sender, RoutedEventArgs e) {
            SelectFolderText.Text = "";
            SelectFolderText.TextAlignment = TextAlignment.Left;
            SelectFolderText.GotFocus -= SelectFolder_GotFocus;
            SelectFolderText.TextChanged += SelectFolderText_TextChanged; 
        }


        private void SelectFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                SelectFolderText.Text = fbd.SelectedPath;
            }
        }

        private void SelectFolderText_TextChanged(object sender, TextChangedEventArgs e) {
            ScanAndRenderDirs();
        }

        private void Confirm_MouseUp(object sender, MouseButtonEventArgs e) {
        }
        private void ScanAndRenderDirs() {
            string path = SelectFolderText.Text;
            panel.Children.Clear();
            if (Directory.Exists(path)) {
                Action a = () => ScanAndRenderDirsBackgroundTask(path);
                Task.Run(a);
            }
        }

        private void ScanAndRenderDirsBackgroundTask(string path) {
            List<string> directories = Directory.GetDirectories(path).ToList();
            foreach (string directory in directories) {
                string name = Path.GetFileName(directory);
                Series s = Series.SearchSingle(name);
                if (s != null) { 
                    Dispatcher.Invoke(new Action(() => {
                        Storyboard sb = (Storyboard)FindResource("OpacityUp");
                        SeriesWithFolder swf = new SeriesWithFolder();
                        swf.Height = 65;
                        swf.SeriesName.Text = s.seriesName;
                        swf.Opacity = 0;
                        swf.FolderLocation.Text = directory;
                        panel.Children.Add(swf);
                        sb.Begin(swf);
                    }), DispatcherPriority.Send);
                }
            }
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.AddPage(new StartUp());
        }

    }
}
