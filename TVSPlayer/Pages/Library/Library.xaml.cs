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
using System.Diagnostics;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Page {
        public Library() {
            InitializeComponent();
            buttons = new LibraryButtons(this);
        }
        LibraryButtons buttons;
        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization custom = new PageCustomization();
            custom.MainTitle = "Library";
            custom.Buttons = buttons;
            custom.SearchBarEvent = (s, ev) => SearchText(MainWindow.GetSearchBarText());
            MainWindow.SetPageCustomization(custom);       
            await ((LibraryButtons)custom.Buttons).SetView();
            var sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => { LoadingText.Visibility = Visibility.Hidden; };
            clone.Begin(LoadingText);
        }



        /// <summary>
        /// Renders posters for library
        /// </summary>
        /// <returns></returns>
        public async Task RenderPosters() {
            PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
            PanelList.Children.RemoveRange(0, PanelList.Children.Count);
            List<Series> allSeries = Database.GetSeries();
            await Task.Run(async() => {
                foreach (Series series in allSeries) {
                    BitmapImage bmp = await Database.GetSelectedPoster(series.id);
                    Dispatcher.Invoke(() => {
                        if (series.id == 321239) { };
                        SeriesInLibrary poster = new SeriesInLibrary(series);
                        poster.PosterImage.Source = bmp;
                        poster.Height = Properties.Settings.Default.LibrarySize;
                        poster.Width = Properties.Settings.Default.LibrarySize / 1.47058823529;
                        poster.RemoveIcon.MouseLeftButtonUp += (s, ev) => RemoveFromLibrary(series, poster);
                        poster.PosterIcon.MouseLeftButtonUp += (s, ev) => SelectPosters(poster, series);
                        poster.QuestionIcon.MouseLeftButtonUp += (s, ev) => { MainWindow.SetPage(new SeriesDetails(series,new Library())); };
                        poster.PosterImage.MouseLeftButtonUp += (s, ev) => MainWindow.SetPage(new SeriesEpisodes(series));
                        PanelPosters.Children.Add(poster);
                    }, DispatcherPriority.Send);

                }

            });
             
          
        }
        public async Task RenderList() {
            PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
            PanelList.Children.RemoveRange(0, PanelList.Children.Count);
            List<Series> allSeries = Database.GetSeries();            
            await Task.Run(() => {
                foreach (Series series in allSeries) {
                    Dispatcher.Invoke(() => {
                        SeriesInLibraryList sil = new SeriesInLibraryList(series);
                        sil.Height = 60;
                        sil.RemoveIcon.MouseUp += (s, ev) => RemoveFromLibrary(series, sil);
                        sil.PosterIcon.MouseUp += (s, ev) => SelectPosters(null, series);
                        sil.Detail.MouseUp += (s, ev) => { MainWindow.SetPage(new SeriesDetails(series, new Library())); };
                        sil.Opacity = 0;
                        sil.Left.MouseLeftButtonUp += (s, ev) => MainWindow.SetPage(new SeriesEpisodes(series));
                        PanelList.Children.Add(sil);
                        var sb = (Storyboard)FindResource("OpacityUp");
                        sb.Begin(sil);
                    }, DispatcherPriority.Send);
                    Thread.Sleep(16);
                }

            });

        }


        public async void SelectPosters(SeriesInLibrary sil, Series series) {
            Poster poster = await MainWindow.SelectPoster(series.id);
            await Task.Run(async () => {
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

        private async void RemoveFromLibrary(Series series, FrameworkElement element) {
            MessageBoxResult result = await MessageBox.Show("This will delete Series from library.\n\nDo you also want to delete the files?","Warning",MessageBoxButtons.YesNoCancel);
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

        private void SearchText(string text) {
            if (buttons.viewPosters == true) {
                foreach (SeriesInLibrary series in PanelPosters.Children) {
                    series.Visibility = Visibility.Visible;
                    if (!series.series.seriesName.ToLower().Contains(text.ToLower())) {
                        series.Visibility = Visibility.Collapsed;
                    }
                }
            } else {
                foreach (SeriesInLibraryList series in PanelList.Children) {
                    series.Visibility = Visibility.Visible;
                    if (!series.series.seriesName.ToLower().Contains(text.ToLower())) {
                        series.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void SortAlphaPosters(bool alpha) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibrary> silList = new List<SeriesInLibrary>();
                UIElementCollection series = PanelPosters.Children;
                foreach (UIElement ui in series) {
                    silList.Add((SeriesInLibrary)ui);
                }
                PanelPosters.Children.RemoveRange(0, PanelPosters.Children.Count);
                silList = alpha ? silList.OrderBy(x => x.series.seriesName).ToList() : silList.OrderBy(x => x.series.seriesName).Reverse().ToList();
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
            Dictionary<SeriesInLibrary, string> sorted = new Dictionary<SeriesInLibrary, string>();
            foreach (SeriesInLibrary sil in list) {
                Episode ep = new Episode();
                List<Episode> li = Database.GetEpisodes(sil.series.id).OrderBy(x => x.firstAired).Reverse().ToList(); ;
                foreach (Episode e in li) {
                    if (!String.IsNullOrEmpty(e.firstAired)) {
                        DateTime dt = Helper.ParseAirDate(e.firstAired).AddDays(1);
                        if (dt < DateTime.Now) {
                            ep = e;
                            break;
                        }
                    }
                }
                sorted.Add(sil, ep.firstAired);
            }
            return sorted.OrderBy(x => x.Value).Reverse().ToDictionary(x => x.Key, x => x.Value).Keys.ToList();           
        }

        public void SortAlphaList(bool alpha) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibraryList> silList = new List<SeriesInLibraryList>();
                foreach (SeriesInLibraryList ui in PanelList.Children) {
                    silList.Add(ui);
                }
                PanelList.Children.RemoveRange(0, PanelList.Children.Count);
                silList = alpha ? silList.OrderBy(x => x.series.seriesName).ToList() : silList.OrderBy(x => x.series.seriesName).Reverse().ToList();
                foreach (SeriesInLibraryList sil in silList) {
                    PanelList.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(PanelList);
            };
            clone.Begin(PanelList);

        }

       

        public void SortCalendarList() {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, e) => {
                List<SeriesInLibraryList> silList = new List<SeriesInLibraryList>();
                foreach (SeriesInLibraryList ui in PanelList.Children) {
                    silList.Add(ui);
                }
                PanelList.Children.RemoveRange(0, PanelList.Children.Count);
                silList = SortEpisodeList(silList);
                foreach (SeriesInLibraryList sil in silList) {
                    PanelList.Children.Add(sil);
                }
                sb = (Storyboard)FindResource("OpacityUp");
                sb.Begin(PanelPosters);
            };
            clone.Begin(PanelPosters);

        }

        private List<SeriesInLibraryList> SortEpisodeList(List<SeriesInLibraryList> list) {
            Dictionary<SeriesInLibraryList, string> sorted = new Dictionary<SeriesInLibraryList, string>();
            foreach (SeriesInLibraryList sil in list) {
                Episode ep = new Episode();
                List<Episode> li = Database.GetEpisodes(sil.series.id).OrderBy(x => x.firstAired).Reverse().ToList(); ;
                foreach (Episode e in li) {
                    if (!String.IsNullOrEmpty(e.firstAired)) {
                        DateTime dt = Helper.ParseAirDate(e.firstAired).AddDays(1);
                        if (dt < DateTime.Now) {
                            ep = e;
                            break;
                        }
                    }
                }
                sorted.Add(sil, ep.firstAired);
            }
            return sorted.OrderBy(x => x.Value).Reverse().ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        }


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
