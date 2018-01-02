using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class Statistics : Page {
        public Statistics() {
            InitializeComponent();
        }

        private async void Base_Loaded(object sender, RoutedEventArgs e) {
            List<SeriesStatistics> stats = new List<SeriesStatistics>();
            double totalSize = 0;
            await Task.Run(() => {
                var list = Database.GetSeries();
                foreach (var item in list) {
                    SeriesStatistics statistics = new SeriesStatistics();
                    statistics.Files = new DirectoryInfo(item.libraryPath).GetFiles("*.*", SearchOption.AllDirectories).ToList();
                    statistics.Series = item;
                    statistics.Files.ForEach(x => {
                        totalSize += x.Length;
                        statistics.Size += x.Length;
                    });
                    stats.Add(statistics);
                }
                double totalTemp = 0;
                for (int i = 0; i < stats.Count; i++) {
                    totalTemp += stats[i].SizePercentage = (stats[i].Size / totalSize) * 100;
                }
                stats = stats.OrderByDescending(x => x.SizePercentage).ToList();
                for (int i = stats.Count - 1; i >= 0; i--) {
                    var percentage = (stats[i].SizePercentage / stats[0].SizePercentage) * 100;
                    if (percentage < 5) {
                        stats.RemoveAt(i);
                    } else {
                        stats[i].RecalculatedPercentage = percentage;
                    }
                }
               
            });
            for (int i = 0; i < stats.Count; i++) {
                var baseColumn = new ColumnDefinition();
                Grid baseGrid = new Grid();
                var spacerLeft = new ColumnDefinition();
                var spacerRight = new ColumnDefinition();
                spacerLeft.Width = new GridLength(250 / stats.Count, GridUnitType.Star);
                spacerRight.Width = new GridLength(250 / stats.Count, GridUnitType.Star);
                baseColumn.Width = new GridLength(1000 / stats.Count, GridUnitType.Star);
                Base.ColumnDefinitions.Add(spacerLeft);
                Base.ColumnDefinitions.Add(baseColumn);
                Base.ColumnDefinitions.Add(spacerRight);
                Base.Children.Add(baseGrid);
                Grid.SetColumn(baseGrid, (i + (i * 2)) + 1);
                var topRow = new RowDefinition();
                topRow.Height = new GridLength(100 - stats[i].RecalculatedPercentage, GridUnitType.Star);
                var bottomRow = new RowDefinition();
                bottomRow.Height = new GridLength(stats[i].RecalculatedPercentage, GridUnitType.Star);
                baseGrid.RowDefinitions.Add(topRow);
                baseGrid.RowDefinitions.Add(bottomRow);
                Grid statGrid = new Grid();            
                statGrid.ToolTip = stats[i].Series.seriesName + ", " + GetSize(stats[i].Size) + ", " + stats[i].SizePercentage.ToString("N2").Replace(",",".") + "%";
                statGrid.Background = (Brush)FindResource("AccentColor");
                baseGrid.Children.Add(statGrid);
                Grid.SetRow(statGrid, 1);
            }
            TopText.Text = "Total disk usage: " + GetSize(totalSize);
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, 245, new TimeSpan(0, 0, 0, 0, 500));
            Base.BeginAnimation(Grid.HeightProperty, doubleAnimation);
            Base.BeginStoryboard((Storyboard)FindResource("OpacityUp"));
            
        }
        private string GetSize(double speed) {
            string speedText = speed + " B/s";
            if (speed >  Math.Pow(1024, 1)) {
                speedText = (speed / Math.Pow(1024, 1)).ToString("N0").Replace(",",".") + " kB";
            }
            if (speed > Math.Pow(1024, 2)) {
                speedText = (speed / Math.Pow(1024, 2)).ToString("N2").Replace(",", ".") + " MB";
            }
            if (speed > Math.Pow(1024, 3)) {
                speedText = (speed / Math.Pow(1024, 3)).ToString("N2").Replace(",", ".") + " GB";
            }
            if (speed > Math.Pow(1024, 4)) {
                speedText = (speed / Math.Pow(1024,4)).ToString("N3").Replace(",", ".") + " TB";
            }
            return speedText;
        }

        class SeriesStatistics {
            public long Size { get; set; }
            public Series Series { get; set; }
            public double SizePercentage { get; set; }
            public double RecalculatedPercentage { get; set; }
            public List<FileInfo> Files { get; set; }
        }
    }
}
