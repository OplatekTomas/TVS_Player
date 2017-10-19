using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SearchShowResult.xaml
    /// </summary>
    public partial class SearchShowResult : UserControl {
        public SearchShowResult(int id) {
            InitializeComponent();
            this.id = id;
        }
        public int id;
        bool showingDetails = false;
        private void Detail_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!showingDetails) {
                Storyboard open = FindResource("OpenDetails") as Storyboard;
                open.Completed += (se, ev) => { DetailsPart.Visibility = Visibility.Visible; };
                open.Begin(MainPart);
                int id = this.id;
                Details.LoadInfo(id);
                showingDetails = true;
                BGGrid.Background = (Brush)FindResource("LighterBG");
            } else {
                DetailsPart.Visibility = Visibility.Hidden;
                Details.HideInfo();
                Storyboard close = FindResource("CloseDetails") as Storyboard;
                close.Begin(MainPart);
                showingDetails = false;
                BGGrid.Background = new BrushConverter().ConvertFromString("#00000000") as SolidColorBrush;
            }
        }
        
    }
}
