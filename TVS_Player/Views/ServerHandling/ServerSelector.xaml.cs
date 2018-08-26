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
using TVS_Player.Properties;
using TVS_Player_Base;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ServerSelector.xaml
    /// </summary>
    public partial class ServerSelector : Page {
        public ServerSelector() => InitializeComponent();

        private void Border_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            View.SetPageCustomization(new ViewCustomization() { SearchBarVisible = false });
            ServerFinder finder = new ServerFinder((s, ev) => {
                ServerResult result = new ServerResult(s.ToString());
                result.MouseLeftButtonUp += async (s1, ev1) => {
                    var res = s.ToString().Split(':');
                    if (await Api.Connect(res[0], Int32.Parse(res[1]))) {
                        Settings.Default.ServerPort = Int32.Parse(res[1]);
                        Settings.Default.ServerIp = res[0];
                        Settings.Default.Save();
                        View.RemovePage();
                    }
                };
                result.Margin = new Thickness(0, 35, 0, 0);
                Animate.ResetMargin(result);
                Animate.FadeIn(result);
                ResultPanel.Children.Add(result);
            });
            await finder.RunPortScanAsync();
            LoadingIcon.Stop();
            Animate.FadeOut(ScanningNetworkGrid);
            ThicknessAnimation anim = new ThicknessAnimation(new Thickness(10, 0, 10, 10), TimeSpan.FromMilliseconds(100));
            ResultsBorder.BeginAnimation(Border.MarginProperty, anim);
            View.SetPageCustomization(new ViewCustomization() { SearchBarVisible = false });
        }



        private async void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!string.IsNullOrEmpty(Address.Text) && !string.IsNullOrEmpty(Port.Text) && Int32.TryParse(Port.Text, out int port)) {
                if (await Api.Connect(Address.Text, port)) {
                    Settings.Default.ServerPort = port;
                    Settings.Default.ServerIp = Address.Text;
                    Settings.Default.Save();
                    View.RemovePage();
                    View.SetPageCustomization(new ViewCustomization() { SearchBarVisible = false });
                }
            }
        }
    }
}
