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

namespace TVS_Player {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() => InitializeComponent();

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            bool connected = await Api.Connect("192.168.1.83", 5850);
            Api.Disconnected += (s, ev) => { MessageBox.Show("Disconnected"); };
            await Api.Login("test", "test");
        }

        private async void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var result = await Api.Search("sky");
        }
    }
}
