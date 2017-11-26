using System;
using System.Collections.Generic;
using System.Globalization;
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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for ScheduleMonth.xaml
    /// </summary>
    public partial class ScheduleMonth : Page {
        public ScheduleMonth(DateTime dateTime, Dictionary<Episode,Series> episodes) {
            InitializeComponent();
            this.dateTime = dateTime;
            this.episodes = episodes;
        }
        DateTime dateTime;
        Dictionary<Episode, Series> episodes;

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            episodes = episodes.Where(x => DateTime.ParseExact(x.Key.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).Month == dateTime.Month && DateTime.ParseExact(x.Key.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).Year == dateTime.Year).ToDictionary(x => x.Key, x => x.Value);
            GenerateResults();

        }

        private async void GenerateResults() {
            int numberOfDays = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            var start = GetStart();
            int current = 1;
            for (int week = 1; week < 6; week++) {
                for (int day = 0; day < 7; day++) {
                    ScheduleDay sd = null;
                    if ((!(day < start) || week != 1) && current <= numberOfDays) {
                        dateTime = new DateTime(dateTime.Year, dateTime.Month, current);
                        var eps = episodes.Where(x => DateTime.ParseExact(x.Key.firstAired, "yyyy-MM-dd", CultureInfo.InvariantCulture).Day == dateTime.Day).ToDictionary(x => x.Key, x => x.Value);
                        sd = new ScheduleDay(eps);
                        int count = 0;
                        var se = eps.Values.Distinct().ToList();
                        foreach (var s in se) {
                            RowDefinition row = new RowDefinition();
                            sd.BaseGrid.RowDefinitions.Add(row);
                            await Task.Run(async () => {
                                var bmp = await Database.GetBanner(s.id);
                                Dispatcher.Invoke(() => {
                                    Grid grid = new Grid();
                                    grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#88000000"));
                                    grid.MouseLeftButtonUp += (sx, ev) => {
                                        Window main = Application.Current.MainWindow;
                                        ((MainWindow)main).SetLibrary();
                                        MainWindow.SetPage(new SeriesEpisodes(s));
                                        };
                                    grid.MouseEnter += (sx, ev) => { grid.BeginStoryboard((Storyboard)FindResource("OpacityDown")); Mouse.OverrideCursor = Cursors.Hand; };
                                    grid.MouseLeave += (sx, ev) => { grid.BeginStoryboard((Storyboard)FindResource("OpacityUp")); Mouse.OverrideCursor = null; };
                                    Image img = new Image();
                                    RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                                    img.Stretch = Stretch.UniformToFill;
                                    img.HorizontalAlignment = HorizontalAlignment.Center;
                                    img.Source = bmp;
                                    img.ToolTip = s.seriesName;
                                    sd.BaseGrid.Children.Add(img);
                                    sd.BaseGrid.Children.Add(grid);
                                    Grid.SetRow(grid, count);
                                    Grid.SetRow(img, count);

                                });

                            });
                            count++;
                        }
                        sd.Day.Text = current.ToString();
                        current++;
                    } else {
                        sd = new ScheduleDay();
                    }
                    BaseCalendar.Children.Add(sd);
                    Grid.SetRow(sd, week);
                    Grid.SetColumn(sd, day);
                }
            }
        }


        private int GetStart() {
            var day = ((int)dateTime.DayOfWeek)-1;
            day = day == -1 ?  6 :day;
            return day;
        }


    }
}
