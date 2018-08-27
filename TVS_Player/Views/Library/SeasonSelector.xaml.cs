using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for SeasonSelector.xaml
    /// </summary>
    public partial class SeasonSelector : UserControl {

        int NumberOfSeasons { get; set; }
        event EventHandler SeasonSelected;
        Grid selected;


        public SeasonSelector(int numberOfSeasons ,EventHandler seasonSelected) {
            InitializeComponent();
            NumberOfSeasons = numberOfSeasons;
            SeasonSelected = seasonSelected;
            LoadUI();
        }

        private void LoadUI() {
            for (int i = 1; i <= NumberOfSeasons; i++) {
                Grid grid = new Grid {
                    Width = 100,
                    Background = Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                Grid underline = new Grid {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Background = (Brush)FindResource("AccentColor"),
                    Height = 2,
                    Opacity = 0
                };
                TextBlock text = new TextBlock {
                    Text = "Season: " + i,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    Foreground = (Brush)FindResource("TextColor"),
                    Margin = new Thickness(0, 0, 0, 10)
                };
                grid.MouseEnter += (s, ev) => Mouse.OverrideCursor = Cursors.Hand;
                grid.MouseLeave += (s, ev) => Mouse.OverrideCursor = null;
                grid.MouseLeftButtonUp += (s, ev) => { SelectSeason(SeasonPanel.Children.IndexOf((UIElement)s) + 1); };
                grid.Children.Add(underline);
                grid.Children.Add(text);
                SeasonPanel.Children.Add(grid);
            }
        }

        public void SelectSeason(int season) {
            if (selected != null) {
                Animate.FadeOut((FrameworkElement)selected.Children[0]);
            }
            selected = (Grid)SeasonPanel.Children[season - 1];
            Animate.FadeIn((FrameworkElement)selected.Children[0]);
            SeasonSelected.Invoke(season, new EventArgs());
        }
    }
}
