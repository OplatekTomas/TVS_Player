using System;
using System.Collections.Generic;
using System.Globalization;
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
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : Page {
        public DownloadsView() {
            InitializeComponent();
        }
        List<TorrentUserControl> userControls = new List<TorrentUserControl>();
        List<Series> allSeries = new List<Series>();

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            PageCustomization pg = new PageCustomization();
            pg.MainTitle = "Torrent downloads";
            MainWindow.SetPageCustomization(pg);
            ContentFrame.Content = new CurrentTorrents();
            allSeries = Database.GetSeries();
        }

        private void CurrentDownloads_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ContentFrame.Content = new CurrentTorrents();
            AllGrid.Visibility = FinishedGrid.Visibility = Visibility.Hidden;
            CurrentGrid.Visibility = Visibility.Visible;
        }

        private void FinishedDownloads_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ContentFrame.Content = new FinishedTorrents();
            AllGrid.Visibility = CurrentGrid.Visibility = Visibility.Hidden;
            FinishedGrid.Visibility = Visibility.Visible;

        }

        private void AllDownloads_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ContentFrame.Content = new AllTorrents();
            FinishedGrid.Visibility = CurrentGrid.Visibility = Visibility.Hidden;
            AllGrid.Visibility = Visibility.Visible;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void CurrentDownloads_MouseEnter(object sender, MouseEventArgs e) {

        }

        private void SeriesNameInput_TextChanged(object sender, TextChangedEventArgs e) {
            Panel.Children.Clear();
            string text = SeriesNameInput.Text.ToLower();
            if (!String.IsNullOrEmpty(text)) { 
                foreach (var series in allSeries) {
                    if (series.seriesName.ToLower() == text) {
                        Clicked(series);
                        break;
                    }
                    if (series.seriesName.ToLower().Contains(text)) {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = series.seriesName;
                        textBlock.Margin = new Thickness(5, 10, 0, 0);
                        textBlock.Height = 30;
                        textBlock.MouseLeftButtonUp += (s, ev) => {
                            SeriesNameInput.Text = series.seriesName;
                            };
                        textBlock.FontSize = 16;
                        textBlock.SetResourceReference(ForegroundProperty, "TextColor");
                        Panel.Children.Add(textBlock);
                    }
                }
            }
        }

        private Series series;
        private Episode episode;
        private void Clicked(Series series) {
            this.series = series;
            Panel.Children.Clear();
            var epi = Database.GetEpisodes(series.id);
            int season = (int)epi.Where(x => !String.IsNullOrEmpty(x.firstAired) && Helper.ParseAirDate(x.firstAired).AddDays(1) < DateTime.Now).Max(x => x.airedSeason);
            int episode = (int)epi.Where(x => x.airedSeason == season && !String.IsNullOrEmpty(x.firstAired) && Helper.ParseAirDate(x.firstAired).AddDays(1) < DateTime.Now).Max(x => x.airedEpisodeNumber);
            this.episode = Database.GetEpisode(series.id, season, episode);
            EpisodeInput.Text = episode.ToString();
            SeasonInput.Text = season.ToString();
            EpisodeInput.TextChanged += async (s, ev) => {
                if (!String.IsNullOrEmpty(SeasonInput.Text) && Int32.TryParse(SeasonInput.Text, out int result)) {
                    int ep = (int)epi.Where(x => x.airedSeason == result && !String.IsNullOrEmpty(x.firstAired) && Helper.ParseAirDate(x.firstAired).AddDays(1) < DateTime.Now).Max(x => x.airedEpisodeNumber);
                    if (!(await IsBetween(EpisodeInput.Text, ep))) {
                        EpisodeInput.Text = "";
                        this.episode = null;
                    } else {
                        if (!String.IsNullOrEmpty(EpisodeInput.Text)) {
                            this.episode = Database.GetEpisode(series.id, result, Int32.Parse(EpisodeInput.Text));
                        }
                    }
                }

            };
            SeasonInput.TextChanged += async (s, ev) => {
                if (!(await IsBetween(SeasonInput.Text, season))) {
                    SeasonInput.Text = "";
                    this.episode = null;
                }
            };

        }

        private async Task<bool> IsBetween(string text, int maxValue) {
            if (String.IsNullOrEmpty(text)) {
                return true;
            }
            if (Int32.TryParse(text, out int result)) {
                if (result > 0 && result <= maxValue) {
                    return true;
                } else {
                    await MessageBox.Show("Number is not in range 1-" + maxValue);
                }
            } else {
                await MessageBox.Show("Input is not number");
            }
            return false;
        }

        private void Panel_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Panel_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private async void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            List<TorrentQuality> tq = new List<TorrentQuality>();
            if ((bool)HD.IsChecked) tq.Add(TorrentQuality.HD);
            if ((bool)FHD.IsChecked) tq.Add(TorrentQuality.FHD);
            if ((bool)STD.IsChecked) tq.Add(TorrentQuality.Standart);
            if (episode != null && series != null) {
                TorrentPanel.Children.Clear();
                MainWindow.AddPage(new PleaseWait());
                List<Torrent> torrents = new List<Torrent>();
                if (tq.Count == 0) { 
                    torrents = await Torrent.Search(series, episode);
                }
                foreach (var item in tq) {
                    var result = await Torrent.Search(series, episode, item);
                    if (result != null) {
                        torrents.AddRange(result);
                    }
                }
                torrents = torrents.OrderByDescending(x => x.Seeders).ToList();
                MainWindow.RemovePage();
                await Task.Run(() => {
                    foreach (var torrent in torrents) {
                        Dispatcher.Invoke(() => {
                            TorrentSearchResult tsr = new TorrentSearchResult(torrent);
                            tsr.Opacity = 0;
                            tsr.Margin = new Thickness(5, 5, 0, 10);
                            tsr.Height = 65;
                            TorrentPanel.Children.Add(tsr);
                            var sb = (Storyboard)FindResource("OpacityUp");
                            sb.Begin(tsr);
                        });
                        Thread.Sleep(16);
                }
                });
            } else {
                await MessageBox.Show("Input cannot be empty");
            }
        }
    }
}
