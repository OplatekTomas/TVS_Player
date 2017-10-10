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
            SelectFolderText.GotFocus -= SelectFolder_GotFocus;
        }


        private void SelectFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                SelectFolderText.Text = fbd.SelectedPath;
                ScanAndRenderDirs();
            }
        }

        private void Confirm_MouseUp(object sender, MouseButtonEventArgs e) {
            ScanAndRenderDirs();
        }
        private void ScanAndRenderDirs() {
            string path = SelectFolderText.Text;
            if (Directory.Exists(path)) {
                Action a = () => ScanAndRenderDirsBackgroundTask(path);
                Task.Run(a);
            } else {
                MessageBox.Show("Directory: " + path + " doesn't exist");
            }
        }

        private void ScanAndRenderDirsBackgroundTask(string path) {
            List<string> directories = Directory.GetDirectories(path).ToList();
            foreach (string directory in directories) {
                Series s = Series.SearchSingle(Path.GetFileName(directory));
                Dispatcher.Invoke(new Action(() => {
                    SeriesWithFolder swf = new SeriesWithFolder();
                    swf.Height = 65;
                    swf.SeriesName.Text = s.seriesName;
                    swf.FolderLocation.Text = directory;
                    panel.Children.Add(swf);
                }), DispatcherPriority.Send);
            }
        }
    }
}
