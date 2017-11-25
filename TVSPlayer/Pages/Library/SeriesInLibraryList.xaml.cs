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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SeriesInLibraryList.xaml
    /// </summary>
    public partial class SeriesInLibraryList : UserControl {
        public SeriesInLibraryList(Series series) {
            InitializeComponent();
            this.series = series;
        }

        public Series series;
        private void Detail_MouseUp(object sender, MouseButtonEventArgs e) {

        }

        private void BGGrid_Loaded(object sender, RoutedEventArgs e) {
            SeriesName.Text = series.seriesName;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
