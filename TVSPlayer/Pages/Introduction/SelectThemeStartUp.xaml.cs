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

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for SelectThemeStartUp.xaml
    /// </summary>
    public partial class SelectThemeStartUp : Page
    {
        public SelectThemeStartUp()
        {
            InitializeComponent();
        }


        private void Dark_MouseUp(object sender, MouseButtonEventArgs e) {
            Settings.Theme = false;
            MainWindow.RemovePage();
            MainWindow.AddPage(new InitialSettings());
        }

        private void Light_MouseUp(object sender, MouseButtonEventArgs e) {
            Settings.Theme = true;
            ThemeSwitcher.SwitchTheme();
            MainWindow.RemovePage();
            MainWindow.AddPage(new InitialSettings());
        }
    }
}
