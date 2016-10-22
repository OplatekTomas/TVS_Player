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
using System.Windows.Forms;
using System.IO;
using Ookii.Dialogs.Wpf;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for AddShows.xaml
    /// </summary>
    public partial class AddShows : Window {
        public AddShows() {
            InitializeComponent();
        }

        private void selectFolder(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            var check = ofd.ShowDialog();
            string path;
            if (showLocation.Text != null & showLocation.Text != "") {
                path = ofd.SelectedPath;
                showLocation.Text = path;
            }
        }

        private void showLocation_TextChanged(object sender, TextChangedEventArgs e) {
            
        }
    }
}
