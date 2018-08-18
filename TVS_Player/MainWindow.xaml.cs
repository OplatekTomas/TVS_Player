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
using TVS_Player_Base;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace TVS_Player {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() => InitializeComponent();

        private async void Window_Loaded(object sender, RoutedEventArgs e) {

        }

        private void MoveWindow(object sender, MouseButtonEventArgs e) {
            DragMove();
        }
        private void HandPointer(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void NormalPointer(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }


        private void TopBar_MouseEnter(object sender, MouseEventArgs e) {
            SearchBar.BeginStoryboard((Storyboard)FindResource("FadeInSearchBar"));
        }

        private void TopBar_MouseLeave(object sender, MouseEventArgs e) {
            SearchBar.BeginStoryboard((Storyboard)FindResource("FadeOutSearchBar"));
        }

        private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void ExitButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Close();
        }

        private void MaximizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (WindowState == WindowState.Maximized) {
                WindowState = WindowState.Normal;
            } else {
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 7;
                WindowState = WindowState.Maximized;
            }
        }
    }
}
