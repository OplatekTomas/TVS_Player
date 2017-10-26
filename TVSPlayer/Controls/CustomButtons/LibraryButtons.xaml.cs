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
    /// Interaction logic for LibraryButtons.xaml
    /// </summary>
    public partial class LibraryButtons : UserControl
    {
        public LibraryButtons(Library library)
        {
            InitializeComponent();
            lib = library;
        }
        private Library lib;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            lib.SetSize(Slider.Value);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Slider.Value = Properties.Settings.Default.LibrarySize;
            Slider.ValueChanged += Slider_ValueChanged;
        }

        private void SortAlpha_MouseUp(object sender, MouseButtonEventArgs e) {
            lib.SortAlpha();
        }

        private void SortReverse_MouseUp(object sender, MouseButtonEventArgs e) {
            lib.SortReverse();
        }

        private void SortCalendar_MouseUp(object sender, MouseButtonEventArgs e) {
            lib.SortCalendar();
        }

        private void ViewPosters_MouseUp(object sender, MouseButtonEventArgs e) {

        }

        private void ViewList_MouseUp(object sender, MouseButtonEventArgs e) {

        }
    }
}
