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

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for SeriesInLibrary.xaml
    /// </summary>
    public partial class SeriesInLibrary : UserControl
    {
        public SeriesInLibrary(Series series)
        {
            InitializeComponent();
            this.series = series;
        }
        public Series series;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PosterImage.Source = Database.GetSelectedPoster(series.id);
        }
    }
}
