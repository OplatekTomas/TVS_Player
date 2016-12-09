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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ScanLocation.xaml
    /// </summary>
    public partial class ScanLocation : Page {
        public ScanLocation() {
            InitializeComponent();
        }
        int index=0;
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

        private void scan_Click(object sender, RoutedEventArgs e) {
            List<string> locs = new List<string>();
            foreach (FolderControl fc in panel.Children) {
                if (!locs.Contains(fc.pathBox.Text)) { 
                    locs.Add(fc.pathBox.Text);
                }
            }

        }
        
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
    }
}
