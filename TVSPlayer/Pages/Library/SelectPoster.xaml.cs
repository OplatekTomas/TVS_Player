using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SelectPoster.xaml
    /// </summary>
    public partial class SelectPoster : Page {
        public SelectPoster(int id) {
            InitializeComponent();
            this.id = id;
        }
        int id;
        Poster currentPoster;

        Poster poster;
        /// <summary>
        /// Asynchronous task that waits until variable show is not null and then returns this variable
        /// </summary>
        public async Task<Poster> ReturnTVShowWhenNotNull() {
            Poster p = null;
            await Task.Run(() => {
                do {
                    Thread.Sleep(100);
                } while (poster == null);
                p = poster;
            });
            poster = null;
            return p;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            if (currentPoster != null) {
                poster = currentPoster;
            } else {
                MessageBox.Show("No poster selected");
            }
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e) {
            List<Poster> list = Database.GetPosters(id);
            await Task.Run(async () => {
                if (list.Count > 0) {
                    Dictionary<Poster, BitmapImage> dic = await GetPosters(list);
                    Dispatcher.Invoke(() => {
                        var sb = (Storyboard)FindResource("OpacityDown");
                        var temp = sb.Clone();
                        temp.Completed += (s, ev) => { LoadingText.Visibility = Visibility.Collapsed; };
                        temp.Begin(LoadingText);
                    }, DispatcherPriority.Send);
                    foreach (var item in dic) {
                        Dispatcher.Invoke(() => {
                            PosterSelection ps = new PosterSelection(item.Key, item.Value);
                            ps.Background.MouseUp += (s, ev) => OnClick(ps);
                            ps.Width = 208;
                            ps.Opacity = 0;
                            var stb = (Storyboard)FindResource("OpacityUp");
                            stb.Begin(ps);
                            Panel.Children.Add(ps);
                        }, DispatcherPriority.Send);
                        Thread.Sleep(16);
                    }
                } else {
                    Poster p = new Poster();
                    p.fileName = "kua";
                    poster = p;
                }
            });
          
        }
        private async Task<Dictionary<Poster, BitmapImage>> GetPosters(List<Poster> posters) {
            Dictionary<Poster, BitmapImage> dic = new Dictionary<Poster, BitmapImage>();
            await Task.Run(() => {
                List<Task> tasks = new List<Task>();
                foreach (Poster poster in posters) {
                    Task t = new Task(()=> {
                        string url = String.IsNullOrEmpty(poster.thumbnail) ? poster.fileName : poster.thumbnail;
                        WebClient wc = new WebClient();
                        var result = wc.DownloadData("https://www.thetvdb.com/banners/" + url);
                        BitmapImage bitmap = new BitmapImage();
                        using (var ms = new MemoryStream(result)) {
                            bitmap.BeginInit();
                            bitmap.StreamSource = ms;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();
                        }
                        dic.Add(poster, bitmap);
                    });
                    tasks.Add(t);
                    t.Start();
                }
                tasks.WaitAll();
            });
            return dic.OrderBy(x => x.Key.ratingsInfo.average).ThenBy(x => x.Key.ratingsInfo.count).Reverse().ToDictionary(t=>t.Key,t=>t.Value);
        }

        private void OnClick(PosterSelection poster) {
            foreach (PosterSelection selection in Panel.Children) {
                if (selection.poster.id != poster.poster.id && selection.selected) {                 
                    selection.Background_MouseUp(new object(), null);
                }
            }
            currentPoster = poster.selected ? poster.poster : null;
        }
    }
}
