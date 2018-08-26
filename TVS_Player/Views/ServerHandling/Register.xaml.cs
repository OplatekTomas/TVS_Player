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

namespace TVS_Player { 
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page {

        public Register() => InitializeComponent();

        private void Grid_Loaded(object sender, RoutedEventArgs e) => View.SetPageCustomization(new ViewCustomization { SearchBarVisible = false });

        private void MainButton_MouseEnter(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Hand;

        private void MainButton_MouseLeave(object sender, MouseEventArgs e) => Mouse.OverrideCursor = null;

        private async void MainButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (Pass.Password == PassAgain.Password) {
                var (loggedin, message, token) = await Api.Register(Username.Text, Pass.Password);
                if (loggedin) {
                    View.SetPage(new Library());
                    Settings.Default.AuthToken = token;
                    Settings.Default.Save();
                    View.ClearHistory();
                } else {
                    ErrorMessage.Text = message;
                }
            } else {
                ErrorMessage.Text = "Passwords don't match.";
            }

        }


    }
}
