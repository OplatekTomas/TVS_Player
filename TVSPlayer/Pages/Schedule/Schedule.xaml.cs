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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for Schedule.xaml
    /// </summary>
    public partial class Schedule : Page {
        public Schedule() {
            InitializeComponent();
        }

        DateTime dateTime = DateTime.Now;
        Dictionary<Episode, Series> episodes = new Dictionary<Episode, Series>();

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization pg = new PageCustomization();
            pg.MainTitle = "Schedule";
            MainWindow.SetPageCustomization(pg);
            foreach (var se in Database.GetSeries()) {
                foreach (var ep in Database.GetEpisodes(se.id).Where(x => !String.IsNullOrEmpty(x.firstAired) && x.airedSeason != 0 && x.airedEpisodeNumber != 0).ToList()) {
                    episodes.Add(ep, se);
                }
            }
            dateTime = new DateTime(dateTime.Year,dateTime.Month,1);
            SetText();
            Content.Content = new ScheduleMonth(dateTime, episodes);
            MonthText.TextChanged += MonthText_TextChanged;
            YearText.TextChanged += YearText_TextChanged;
        }

        private void SetText() {
            MonthText.Text = dateTime.Month.ToString();
            YearText.Text = dateTime.Year.ToString();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void BackMonth_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            dateTime = dateTime.AddMonths(-1);
            SetText();
        }

        private void NextMonth_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            dateTime = dateTime.AddMonths(1);
            SetText();
        }

        private async void MonthText_TextChanged(object sender, TextChangedEventArgs e) {
            if (!String.IsNullOrEmpty(MonthText.Text)) {
                if (Int32.TryParse(MonthText.Text, out int result) && result >= 1 && result <= 12) {
                    string oldText = MonthText.Text;
                    await Task.Delay(1000);
                    if (oldText == MonthText.Text) {
                        dateTime = new DateTime(dateTime.Year, result, 1);
                        Content.Content = new ScheduleMonth(dateTime, episodes);
                    }
                } else {
                    await MessageBox.Show("Input is not between 1-12");
                }
            }
        }

        private async void YearText_TextChanged(object sender, TextChangedEventArgs e) {
            if (!String.IsNullOrEmpty(YearText.Text)) {
                if (Int32.TryParse(YearText.Text, out int result) && result <10000) {
                    string oldText = YearText.Text;
                    await Task.Delay(1000);
                    if (oldText == MonthText.Text) {
                        dateTime = new DateTime(result, dateTime.Month, 1);
                        Content.Content = new ScheduleMonth(dateTime, episodes);
                    }
                } else {
                    await MessageBox.Show("Input is not between 1-9999");
                }
            }
        }
    }
}
