using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using TVS.API;
using System.Linq;
using System.Globalization;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page {
        public Library() {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization custom = new PageCustomization();
            custom.MainTitle = "Library";
            custom.Buttons = new LibraryButtons(this);
            custom.SearchBarEvent = (s, ev) => SearchText(MainWindow.GetSearchBarText());
            MainWindow.SetPageCustomization(custom);
            List<Series> allSeries = Database.GetSeries();
            await Task.Run(() => {
                foreach (Series series in allSeries) {
                    Dispatcher.Invoke(() => {
                        var poster = new SeriesInLibrary(series);
                        poster.Opacity = 0;
                        poster.Height = 295;
                        Storyboard sb = (Storyboard)FindResource("OpacityUp");
                        sb.Begin(poster);              
                        Panel.Children.Add(poster);
                    }, DispatcherPriority.Send);
                    Thread.Sleep(50);
                }

            });
        }

        private void SearchText(string text) {
            foreach (SeriesInLibrary series in Panel.Children) {
                series.Visibility = Visibility.Visible;
                if (!series.series.seriesName.ToLower().Contains(text.ToLower())) {
                    series.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void SortAlpha() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = Panel.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                Panel.Children.RemoveRange(0, Panel.Children.Count);
                silList = silList.OrderBy(x => x.series.seriesName).ToList();
                foreach (SeriesInLibrary sil in silList) {
                    Panel.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(this);
                };
            clone.Begin(this);
            
        }

        public void SortReverse() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = Panel.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                Panel.Children.RemoveRange(0, Panel.Children.Count);
                silList = silList.OrderBy(x => x.series.seriesName).Reverse().ToList();
                foreach (SeriesInLibrary sil in silList) {
                    Panel.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(this);
            };
            clone.Begin(this);
        }

        public void SortCalendar() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = Panel.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                Panel.Children.RemoveRange(0, Panel.Children.Count);
                silList = SortEpisode(silList);
                foreach (SeriesInLibrary sil in silList) {
                    Panel.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(this);
            };
            clone.Begin(this);

        }

        private List<SeriesInLibrary> SortEpisode(List<SeriesInLibrary> list) {
            List<Series> active = new List<Series>();
            Dictionary<Series, string> sorted = new Dictionary<Series, string>();
            List<Tuple<SeriesInLibrary, Series, string>> together = new List<Tuple<SeriesInLibrary, Series, string>>();
            foreach (var sir in list) {
                active.Add(sir.series);
            }
            foreach (Series s in active) {
                Episode ep = new Episode();
                List<Episode> li = Database.GetEpisodes(s.id);
                li = li.OrderBy(x => x.firstAired).Reverse().ToList();
                foreach (Episode e in li) {
                    if (!String.IsNullOrEmpty(e.firstAired)) {
                        DateTime dt = DateTime.ParseExact(e.firstAired,"yyyy-MM-dd",CultureInfo.InvariantCulture);
                        if (dt < DateTime.Now) {
                            ep = e;
                            break;
                        }
                    }
                }
                sorted.Add(s, ep.firstAired);
            }
            foreach (var s in sorted) {
                foreach (var sir in list) {
                    if (s.Key.id == sir.series.id) {
                        together.Add(new Tuple<SeriesInLibrary, Series, string>(sir,s.Key,s.Value));
                    }
                }
            }
            together = together.OrderBy(x => x.Item3).Reverse().ToList();
            var stopit = new List<SeriesInLibrary>();
            foreach (var wtf in together) {
                stopit.Add(wtf.Item1);
            }
            return stopit;
        }

        public void ViewPosters() { }

        public void ViewList() { }

        public void SetSize(double size) {
            Properties.Settings.Default.LibrarySize = size;
            Properties.Settings.Default.Save();
            UIElementCollection uiColl = Panel.Children;
            foreach (SeriesInLibrary ui in uiColl) {
                ui.Height = size;
            }
        }

    }
}
