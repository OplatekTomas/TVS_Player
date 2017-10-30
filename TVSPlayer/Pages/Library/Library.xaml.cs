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
using System.IO;
using System.Net;

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
            //PanelPosters.Opacity = 0;        
            await ((LibraryButtons)custom.Buttons).SetView();
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => {
                LoadingText.Visibility = Visibility.Hidden;
            };
            clone.Begin(LoadingText);
        }



        /// <summary>
        /// Renders posters for library
        /// </summary>
        /// <returns></returns>
        public async Task RenderPosters() {
            PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
            List<Series> allSeries = Database.GetSeries();
            foreach (Series series in allSeries) {
                SeriesInLibrary poster = new SeriesInLibrary(series);
                poster.Height = Properties.Settings.Default.LibrarySize;
                poster.Width = Properties.Settings.Default.LibrarySize / 1.47058823529;
                poster.RemoveIcon.MouseLeftButtonUp += (s,ev) => RemoveFromLibrary(series,poster);
                poster.PosterIcon.MouseLeftButtonUp += (s, ev) => SelectPosters(series,poster);
                PanelPosters.Children.Add(poster);
            }     
          
        }

        public async void SelectPosters(Series series, SeriesInLibrary sil) {
            Poster poster = await MainWindow.SelectPoster(series.id);
            
            await Task.Run( async () => {
                series.defaultPoster = poster;
                Database.EditSeries(series.id, series);
                if (sil != null) {
                    Dispatcher.Invoke(() => {
                        Storyboard sb = (Storyboard)FindResource("OpacityDown");
                        sb.Begin(sil.PosterImage);
                    }, DispatcherPriority.Send);
                    BitmapImage bmp = await Database.GetSelectedPoster(series.id);
                    Dispatcher.Invoke(() => {
                        sil.PosterImage.Source = bmp;
                        Storyboard sb = (Storyboard)FindResource("OpacityUp");
                        sb.Begin(sil.PosterImage);
                    }, DispatcherPriority.Send);
                }
            });         
        }

        public async Task RenderList() {
            PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
            List<Series> allSeries = Database.GetSeries();
            foreach (Series series in allSeries) {
                
            }
        }

        private void SearchText(string text) {
            foreach (SeriesInLibrary series in PanelPosters.Children) {
                series.Visibility = Visibility.Visible;
                if (!series.series.seriesName.ToLower().Contains(text.ToLower())) {
                    series.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void RemoveFromLibrary(Series series, FrameworkElement element) {
            MessageBoxResult result = MessageBox.Show("This will delete Series from library.\n\nDo you also want to delete the files?","Warning",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
            if (result != MessageBoxResult.Cancel) {
                Database.RemoveSeries(series.id);
                while (true) {
                    try {
                        Directory.Delete(Helper.data + series.id, true);
                        break;
                    } catch (IOException e) { Thread.Sleep(15); }
                }
                if (result == MessageBoxResult.Yes) {
                    while (true) {
                        try {
                            Directory.Delete(series.libraryPath, true);
                            break;
                        } catch (IOException e) { Thread.Sleep(15); }
                    }
                }
                Storyboard sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) => {
                    element.Visibility = Visibility.Collapsed;
                };
                clone.Begin(element);
            }


        }

        public void SortAlphaPosters() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = PanelPosters.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
                silList = silList.OrderBy(x => x.series.seriesName).ToList();
                foreach (SeriesInLibrary sil in silList) {
                    PanelPosters.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(PanelPosters);
                };
            clone.Begin(PanelPosters);
            
        }

        public void SortReversePosters() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = PanelPosters.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
                silList = silList.OrderBy(x => x.series.seriesName).Reverse().ToList();
                foreach (SeriesInLibrary sil in silList) {
                    PanelPosters.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(PanelPosters);
            };
            clone.Begin(PanelPosters);
        }

        public void SortCalendarPosters() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = PanelPosters.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
                silList = SortEpisodePosters(silList);
                foreach (SeriesInLibrary sil in silList) {
                    PanelPosters.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(PanelPosters);
            };
            clone.Begin(PanelPosters);

        }

        private List<SeriesInLibrary> SortEpisodePosters(List<SeriesInLibrary> list) {
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
            UIElementCollection uiColl = PanelPosters.Children;
            foreach (SeriesInLibrary ui in uiColl) {
                ui.Height = size;
                ui.Width = size / 1.47058823529;
            }
        }

    }
}
