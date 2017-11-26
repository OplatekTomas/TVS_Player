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
    /// Interaction logic for ScheduleDay.xaml
    /// </summary>
    public partial class ScheduleDay : UserControl {
        public ScheduleDay() {
            InitializeComponent();
        }

        public ScheduleDay(Dictionary<Episode,Series> episodes) {
            InitializeComponent();
            this.episodes = episodes;
        }
        Dictionary<Episode, Series> episodes;

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            if (episodes == null || episodes.Count == 0) {
                //Cover.Opacity = 0;
            }
        }
    }
}
