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
    /// Interaction logic for ShowInfo.xaml
    /// </summary>
    public partial class ShowInfo : Page {

        public Page lastPage;

        public ShowInfo() {
            InitializeComponent();
        }

        private void ReturnBack_Event(object sender, MouseButtonEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(lastPage);
        }
    }
}
