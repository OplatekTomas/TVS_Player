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

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : UserControl {

        private bool _canRotate = true;
        Storyboard animation;

        public Loading(){
            InitializeComponent();
            animation = ((Storyboard)FindResource("Rotate")).Clone();
            animation.Completed += (s, ev) => {
                if (_canRotate) {
                    Start();
                }
            };
            LoadingIcon.BeginStoryboard(animation);
        }
        public void Start() {
            LoadingIcon.RenderTransform = new RotateTransform(1);
            _canRotate = true;
            LoadingIcon.BeginStoryboard(animation);
        }

        public void Stop() {
            _canRotate = false;
        }
    }
}
