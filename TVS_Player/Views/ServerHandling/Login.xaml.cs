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
using TVS_Player.Properties;
using TVS_Player_Base;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login() => InitializeComponent();

        private void MainButton_MouseEnter(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Hand;

        private void MainButton_MouseLeave(object sender, MouseEventArgs e) => Mouse.OverrideCursor = null;

        private void Register_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => View.SetPage(new Register());

        private void Grid_Loaded(object sender, RoutedEventArgs e) => View.SetPageCustomization(new ViewCustomization { SearchBarVisible = false });

        private async void MainButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var (loggedin, message, token) = await Api.Login(Username.Text, Pass.Password);
            if (loggedin) {
                View.SetPage(new Library());
                Settings.Default.AuthToken = token;
                Settings.Default.Save();
                View.ClearHistory();
            } else {
                ErrorMessage.Text = message;
            }
        }

    }
}
