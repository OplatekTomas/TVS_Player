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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for RenameShow.xaml
    /// </summary>
    public partial class RenameShow : Page {
        private ShowRectangle sr;
        public RenameShow() {
            InitializeComponent();
        }

        public RenameShow(ShowRectangle sri) {
            sr = sri;
            InitializeComponent();
            before.Text = sr.ShowName;
        }

        private void Cancel_Event(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();

        }

        private void RenameShow_Event(object sender, RoutedEventArgs e) {
            sr.ShowName = after.Text;
            sr.RegenerateInfo();
            Cancel_Event(this,e);
        }
    }
}
