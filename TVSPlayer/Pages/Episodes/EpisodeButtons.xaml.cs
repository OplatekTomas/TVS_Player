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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;

namespace TVSPlayer
{
    /// <summary>
    /// Interaction logic for EpisodeButtons.xaml
    /// </summary>
    public partial class EpisodeButtons : UserControl
    {
        public EpisodeButtons(SeriesEpisodes owner)
        {
            InitializeComponent();
            this.owner = owner;
        }
        SeriesEpisodes owner;

        private void SeriesDetails_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.SetPage(new SeriesDetails(owner.series, new SeriesEpisodes(owner.series)));
        }

        private async void SelectPoster_MouseUp(object sender, MouseButtonEventArgs e) {
            Poster poster = await MainWindow.SelectPoster(owner.series.id);
            await Task.Run(async () => {
                owner.series.defaultPoster = poster;
                Database.EditSeries(owner.series.id, owner.series);
                Dispatcher.Invoke(() => {
                    Storyboard sb = (Storyboard)FindResource("OpacityDown");
                    sb.Begin(owner.DefaultPoster);
                }, DispatcherPriority.Send);
                BitmapImage bmp = await Database.GetSelectedPoster(owner.series.id);
                Dispatcher.Invoke(() => {
                    owner.DefaultPoster.Source = bmp;
                    Storyboard sb = (Storyboard)FindResource("OpacityUp");
                    sb.Begin(owner.DefaultPoster);
                }, DispatcherPriority.Send);
            });
        }

        private void SwitchAutoDownload_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            DownloadSwitcher();
        }

        public async void DownloadSwitcher() {
            if (Settings.AutoDownload) {
                if (owner.series.autoDownload) {
                    var sb = (Storyboard)FindResource("OpacityUp");
                    sb.Begin(AutoDownloadIcon);
                } else {
                    var sb = (Storyboard)FindResource("OpacityDown");
                    sb.Begin(AutoDownloadIcon);
                }
                owner.series.autoDownload = !owner.series.autoDownload;
                Database.EditSeries(owner.series.id, owner.series);
            } else {
                var result = await MessageBox.Show("Autodownload works using torrent and might be illegal in your country.\nI am not responsible for your actoins.\n\nDo you want to enable autodownload", "Autodownload is disabled", MessageBoxButtons.YesNoCancel);
                if (result == MessageBoxResult.Yes) {
                    Settings.AutoDownload = true;
                    DownloadSwitcher();
                }
            }
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            AutoDownloadIcon.Opacity = owner.series.autoDownload ? 0 : 1;
            SortButtonImage.Source = !Properties.Settings.Default.EpisodeSort ? (BitmapImage)FindResource("AlphabeticalIcon") : (BitmapImage)FindResource("AlphabeticalReverseIcon");
        }

        private async void SortButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) => {
                SortButtonImage.Source = !Properties.Settings.Default.EpisodeSort ? (BitmapImage)FindResource("AlphabeticalIcon") : (BitmapImage)FindResource("AlphabeticalReverseIcon");
                var up = (Storyboard)FindResource("OpacityUp");
                up.Begin(SortButtonImage);
            };
            clone.Begin(SortButtonImage);
            Properties.Settings.Default.EpisodeSort = !Properties.Settings.Default.EpisodeSort;
            Properties.Settings.Default.Save();
            await Task.Run(() => owner.LoadSeasons());
        }

        private void WatchAll_MouseUp(object sender, MouseButtonEventArgs e) {
            var eps = Database.GetEpisodes(owner.series.id);
            foreach (var ep in eps) {
                ep.finised = true;
                Database.EditEpisode(owner.series.id, ep.id, ep);
            }
        }
    }
}
