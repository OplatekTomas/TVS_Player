using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            ((MainWindow)main).CloseTempFrameIndex();
        }

        private void RenameShow_Event(object sender, RoutedEventArgs e) {
            sr.ShowName = after.Text;
            sr.RegenerateInfo(false);
            Cancel_Event(this,e);
        }


        private void ResetText_Event(object sender, RoutedEventArgs e) {
            if (string.Compare(after.Text, "Rename show") == 0) {
                after.Text = string.Empty;
                after.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
    }
}
