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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for ImportScanFolder.xaml
    /// </summary>
    public partial class ImportScanFolder : Page {
        public ImportScanFolder() {
            InitializeComponent();
        }
        private void StartAnimation( string storyboard , Grid pnl ) {
            Storyboard sb = this.FindResource(storyboard) as Storyboard;
            sb.Begin(pnl);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            TranslateTransform t = new TranslateTransform(0, BackGrid.ActualHeight);
            BGrid.MouseUp += Grid_MouseUp;
            Content.RenderTransform = t;
            StartAnimation("SlideToMiddle", Content);
            StartAnimation("OpacityUp", BackGrid);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).RemovePage();
        }
    }
}
