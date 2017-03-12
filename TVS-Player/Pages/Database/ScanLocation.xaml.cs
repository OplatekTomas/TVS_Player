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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ScanLocation.xaml
    /// </summary>
    public partial class ScanLocation : Page {
        bool addDb;
        bool startup = false;
        public ScanLocation(bool addToDb) {
            InitializeComponent();
            addDb = addToDb;
        }
        public ScanLocation(bool addToDb,bool startup) {
            InitializeComponent();
            addDb = addToDb;
            this.startup = startup;
        }
        private void textbox_GotFocus(object sender, RoutedEventArgs e) {
            textbox.Text = string.Empty;
            textbox.GotFocus -= textbox_GotFocus;
        }
        private void textbox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var res = fbd.ShowDialog();
            if (res == DialogResult.OK) {
                AddLocation(fbd.SelectedPath);
            }

        }
        private void add_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(textbox.Text)) {
                AddLocation(textbox.Text);
            }
        }
        private void AddLocation(string path) {
            FolderControl fc = new FolderControl();
            fc.pathBox.Text = path;
            fc.Height = 50;
            fc.editLocation.Click += (s, e) => editOption(fc);
            fc.removeLocation.Click += (s,e) => removeOption(fc);
            panel.Children.Add(fc);
        }
        private void removeOption(FolderControl fc) {               
            panel.Children.Remove(fc);
        }
        private void editOption(FolderControl fc) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            var res = fbd.ShowDialog();
            if (res == DialogResult.OK) {
                fc.pathBox.Text = fbd.SelectedPath;
            }

        }

        private async Task AddToDB(List<int> ids, List<string> locs, ProgressWindow pw) {
            pw.MainText.Text = "Creating database";
            int count = ids.Count;
            await Task.Run(() => {
                for (int i = 0; i < count; i++) {
                    Dispatcher.Invoke(new Action(() => {
                        pw.Progress.Value = i + 1;
                        pw.SecondText.Text = i + 1 + "/" + count;
                    }), DispatcherPriority.Send);
                    Renamer.RenameBatch(new List<int>() { ids[i] }, locs, AppSettings.GetLibLocation());
                }
            });
            pw.Close();
        }

        private async void scan_Click(object sender, RoutedEventArgs e) {
            List<string> locs = new List<string>();
            List<int> ids = new List<int>();
            foreach (FolderControl fc in panel.Children) {
                if (!locs.Contains(fc.pathBox.Text)) {
                    locs.Add(fc.pathBox.Text);
                    AppSettings.AddLocation(fc.pathBox.Text);
                }
            }
            if (!addDb) {
                foreach (Show s in DatabaseShows.ReadDb()) {
                    ids.Add(s.id);
                }
            } else {
                ids.Add(DatabaseShows.ReadDb().Last().id);
            }
            ProgressWindow pw = new ProgressWindow(ids.Count);
            pw.Show();
            await AddToDB(ids, locs, pw);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
            if (startup) {
                ((MainWindow)main).CloseTempFrameIndex();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e) {
            foreach (string path in AppSettings.GetLocations()) {
                FolderControl fc = new FolderControl();
                fc.pathBox.Text = path;
                fc.editLocation.Click += (se, ea) => editOption(fc);
                fc.removeLocation.Click += (se, ea) => removeOption(fc);
                panel.Children.Add(fc);
            }
        }
    }
}
