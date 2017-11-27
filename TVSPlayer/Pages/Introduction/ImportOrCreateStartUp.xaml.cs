using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for StartUp.xaml
    /// </summary>
    public partial class StartUp : Page {
        public StartUp() {
            InitializeComponent();
        }

        private void Import_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.AddPage(new LibraryImport());
        }

        private void Create_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.AddPage(new LibraryCreate());
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.AddPage(new InitialSettings());
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
