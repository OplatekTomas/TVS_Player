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
using TVS_Player_Base;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page
    {
        public Library()
        {
            InitializeComponent();
        }

        private async void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e) {
            var list = await Series.GetSeries();
            foreach (var series in list) {
                SeriesPoster sp = new SeriesPoster();
                sp.MouseEnter += (s, ev) => ItemMouseEnter();
                sp.MouseLeave += (s, ev) => ItemMouseLeave();
                sp.MouseLeftButtonUp += (s,ev) => View.SetPage(new Page());
                sp.PosterImage.Source = await Helper.GetImage(series.URL);
                SeriesPanel.Children.Add(sp);
                Animate.FadeIn(sp);
            }

        }

        private void ItemMouseEnter() {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void ItemMouseLeave() {
            Mouse.OverrideCursor = null;
        }

    }
}
