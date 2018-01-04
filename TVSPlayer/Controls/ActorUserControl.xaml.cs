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
using TVS.API;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for ActorUserControl.xaml
    /// </summary>
    public partial class ActorUserControl : UserControl
    {
        public ActorUserControl(Actor actor)
        {
            InitializeComponent();
            this.actor = actor;
        }
        Actor actor;

        private async void BackgroundGrid_Loaded(object sender, RoutedEventArgs e) {
            Opacity = 0;
            if (!String.IsNullOrEmpty(actor.image)) {
                ActorFace.Source = await Database.LoadImage(new Uri("https://www.thetvdb.com/banners/"+actor.image));
                var sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(this);
            }
        }

        private void Name_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Name_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
