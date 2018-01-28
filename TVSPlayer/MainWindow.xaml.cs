using Ragnar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TVS.API;
using TVS.Notification;
using Timer = System.Timers.Timer;

namespace TVSPlayer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window {

        public MainWindow() {
            if (!Helper.CheckRunning()) {
                Environment.Exit(0);
            }
            InitializeComponent();
            SetDimensions();
        }

        public static bool videoPlayback = false;

        #region Animations

        private void StartAnimation(string storyboard, FrameworkElement grid) {
            Storyboard sb = this.FindResource(storyboard) as Storyboard;
            sb.Begin(grid);
        }

        private void SideButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            HiderGrid.Visibility = Visibility.Visible;
            StartAnimation("ShowSideMenu", SideMenu);
            StartAnimation("OpacityUp", HiderGrid);
        }

        private void HiderGrid_MouseUp(object sender, MouseButtonEventArgs e) {
            HideSideBar();
        }

        private void HideSideBar() {
            StartAnimation("HideSideMenu", SideMenu);
            StartAnimation("OpacityDown", HiderGrid);
            HiderGrid.Visibility = Visibility.Hidden;
        }

        //Shows search bar
        private void SearchButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            StartAnimation("ShowSearch", SearchBar);
            StartAnimation("OpacityUp", SearchBar);
            StartAnimation("MoveSearchLeft", SearchButton); 
            SearchBox.Focus();
        }

        //Hides search bar
        private async void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            StartAnimation("HideSearch", SearchBar);
            StartAnimation("OpacityDown", SearchBar);
            StartAnimation("MoveSearchRight", SearchButton);
            await Task.Run(() => {
                Thread.Sleep(500);
                Dispatcher.Invoke(() => {
                    SearchBox.Text = "";
                }, DispatcherPriority.Send);
            });
        }

        bool rightsidevisible = false;
        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (rightsidevisible) {
                StartAnimation("HideNotification", RightButtons);
                Storyboard sb = (Storyboard)FindResource("OpacityDown");
                Storyboard clone = sb.Clone();
                clone.Completed += (s, ev) => {
                    NotificationArea.Visibility = Visibility.Hidden;
                };
                clone.Begin(NotificationArea);
                rightsidevisible = false;
            } else {
                double rightMargin = CustomContent.Children.Count == 1 ? Int32.Parse(CustomContent.Children[0].GetValue(ActualWidthProperty).ToString()) + 50 : 50;
                ThicknessAnimation animation = new ThicknessAnimation(new Thickness(0, 0, rightMargin, 0), new TimeSpan(0, 0, 0, 0, 250));
                RightButtons.BeginAnimation(MarginProperty, animation);
                NotificationArea.Visibility = Visibility.Visible;
                StartAnimation("OpacityUp", NotificationArea);
                rightsidevisible = true;

            }
        }

        #endregion

        #region Page handling

        public enum PlayerState { PiP, Fullscreen, Normal };
        private class Dimensions {
            public double Width { get; set; }
            public double Height { get; set; }
            public WindowState LastState { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
        }
        Dimensions dimensions;
        PlayerState currentState = PlayerState.Normal;

        public static void SwitchState(PlayerState state ,bool reset = false) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).ViewSwitcher(state ,reset);
        }

        public void ViewSwitcher(PlayerState state, bool reset) {
            if (currentState == state) {
                state = PlayerState.Normal;
            }
            if (currentState == PlayerState.Normal && state == PlayerState.Normal) {
                return;
            }
            if ((currentState == PlayerState.Fullscreen && state == PlayerState.PiP) || ((currentState == PlayerState.PiP && state == PlayerState.Fullscreen))) {
                ViewSwitcher(PlayerState.Normal, reset);
            }
            if (currentState == PlayerState.PiP) {
                this.Top = dimensions.Top;
                this.Left = dimensions.Left;
            }
            switch (state) {
                case PlayerState.Fullscreen:
                    dimensions = new Dimensions() {
                        Width = this.Width,
                        Height = this.Height,
                        LastState = this.WindowState
                    };
                    this.Visibility = Visibility.Collapsed;
                    this.Topmost = true;
                    this.WindowStyle = WindowStyle.None;
                    this.ResizeMode = ResizeMode.NoResize;
                    this.WindowState = WindowState.Maximized;
                    this.Visibility = Visibility.Visible;
                    break;
                case PlayerState.PiP:
                    dimensions = new Dimensions() {
                        Width = this.Width,
                        Height = this.Height,
                        LastState = this.WindowState,
                        Left = this.Left,
                        Top = this.Top
                    };
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Normal;
                    this.MinWidth = 640;
                    this.Width = this.MinWidth;
                    this.MinHeight = 360;
                    this.Height = this.MinHeight;
                    this.Left = SystemParameters.PrimaryScreenWidth - 660;
                    this.Top = 20;
                    this.ResizeMode = ResizeMode.NoResize;
                    this.Topmost = true;
                    break;
                case PlayerState.Normal:
                    this.WindowState = dimensions.LastState;
                    this.Topmost = false;
                    this.MinHeight = 480;
                    this.MinWidth = 800;
                    this.Height = dimensions.Height;
                    this.Width = dimensions.Width;
                    this.ResizeMode = ResizeMode.CanResize;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    dimensions = null;
                    break;
            }
            currentState = state;



        }

        public static string GetSearchBarText() {
            Window main = Application.Current.MainWindow;
            return ((MainWindow)main).SearchBox.Text;
        }

        public static string GetCurrentFrameContentName() {
            Window main = Application.Current.MainWindow;
            return ((MainWindow)main).ActiveContent.Content.GetType().Name;
        }

        /// <summary>
        /// Function that renders new frame and page on top of existing content
        /// </summary>
        /// <param name="page">Page you want to show</param>
        public static void AddPage(Page page) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageCreator(page);      
        }

        public static async Task<Poster> SelectPoster(int id) {
            Window main = Application.Current.MainWindow;
            return await ((MainWindow)main).PosterSelector(id);
        }

        private async Task<Poster> PosterSelector(int id) {
            SelectPoster selectPoster = new SelectPoster(id);
            AddPage(selectPoster);
            Poster s = await selectPoster.ReturnTVShowWhenNotNull();
            if (s.fileName == "kua") {
                return null;
            }
            RemovePage();        
            return s;
        }

        /// <summary>
        /// Call this function when you need to search API (returns either basic info about Series or null)
        /// </summary>
        public static async Task<Series> SearchShow() {
            Window main = Application.Current.MainWindow;
            return await ((MainWindow)main).ShowSearcher();
        }

        /// <summary>
        /// Call this function when you need to search API (returns either basic info about Series or null)
        /// </summary>
        private async Task<Series> ShowSearcher() {
            SearchSingleShow singleSearch = new SearchSingleShow();
            AddPage(singleSearch);
            Series s = await singleSearch.ReturnTVShowWhenNotNull();
            RemovePage();
            singleSearch.show = null;
            if (s.imdbId == "kua") {
                return null;
            }
            return s;
        }

        /// <summary>
        /// Removes last page added
        /// </summary>
        public static void RemovePage() {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageRemover();
        }

        public static void RemoveAllPages() {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageRemoverFull();
        }

        /// <summary>
        /// Sets page in Main Frame to any page
        /// </summary>
        /// <param name="page">What page to set to</param>
        public static void SetPage(FrameworkElement page) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageSetter(page);
            SetPageCustomization(new PageCustomization());
        }

        public static void HideContent() {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).BaseGrid.Visibility = Visibility.Collapsed;
        }

        public static void ShowContent() {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).BaseGrid.Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// Sets stuff from the customization class
        /// </summary>
        /// <param name="customization">Instance of PageCustomization</param>
        public static void SetPageCustomization(PageCustomization customization) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageCustomizationSetter(customization);
        }

        private TextChangedEventHandler lastHandler;

        private void PageCustomizationSetter(PageCustomization custom) {
            StartAnimation("HideNotification", RightButtons);
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard clone = sb.Clone();
            clone.Completed += (s, ev) => {
                NotificationArea.Visibility = Visibility.Hidden;
            };
            clone.Begin(NotificationArea);
            rightsidevisible = false;
            PageTitle.Text = custom.MainTitle;
            if (lastHandler != null) {
                SearchBox.TextChanged -= lastHandler;
            }
            if (custom.SearchBarEvent != null) {
                SearchButton.Visibility = Visibility.Visible;
                lastHandler = custom.SearchBarEvent;
                SearchBox.TextChanged += custom.SearchBarEvent;
            } else {
                SearchButton.Visibility = Visibility.Collapsed;
            }
            if (custom.Buttons != null) {
                CustomContent.Children.RemoveRange(0, CustomContent.Children.Count);
                CustomContent.Width = ((FrameworkElement)custom.Buttons).Width;
                CustomContent.Children.Add(custom.Buttons);
            } else {
                CustomContent.Children.RemoveRange(0, CustomContent.Children.Count);
            }
        }

        private void PageSetter(FrameworkElement page) {
            Storyboard sb = (Storyboard)FindResource("OpacityDown");
            Storyboard temp = sb.Clone();
            temp.Completed += (s,e) => {
                ActiveContent.Content = page;
                Storyboard up = (Storyboard)FindResource("OpacityUp");
                up.Begin(ActiveContent);
            };
            temp.Begin(ActiveContent);
        }

        private void PageCreator(Page page) {
            Frame fr = new Frame();
            Grid.SetRowSpan(fr, 2);
            fr.Opacity = 0;
            fr.Content = page;
            Storyboard sb = this.FindResource("OpacityUp") as Storyboard;
            Storyboard blur = (Storyboard)FindResource("BlurBackground");
            blur.Begin();
            ContentOnTop.Children.Add(fr);
            sb.Begin(fr);
        }

        private void PageRemoverFull() {
            for (int i = 0; i < ContentOnTop.Children.Count; i++) {
                PageRemover();
            }
        }

        private void PageRemover() {
            var p = ContentOnTop.Children[ContentOnTop.Children.Count - 1] as FrameworkElement;
            Storyboard sb = this.FindResource("OpacityDown") as Storyboard;
            Storyboard sbLoad = sb.Clone();
            Storyboard blur = (Storyboard)FindResource("DeBlurBackground");
            blur.Begin();
            sbLoad.Completed += (s, e) => FinishedRemove(p);
            sbLoad.Begin(p);
        }

        private void FinishedRemove(UIElement ue) {
            ContentOnTop.Children.Remove(ue);
        }

        #endregion

        #region Events

        private void ThemeSwitch_MouseUp(object sender, MouseButtonEventArgs e) {
            ThemeSwitcher.SwitchTheme();
        }

        private void SettingsSideBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetPage(new SettingsPage());
            HideSideBar();
            LibrarySelected.Opacity = ScheduleSelected.Opacity = AboutSelected.Opacity = DownloadsSelected.Opacity = StatisticsSelected.Opacity = 0;
            SettingsSelected.Opacity = 1;
        }

        private void AboutSideBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetPage(new AboutView());
            HideSideBar();
            LibrarySelected.Opacity = ScheduleSelected.Opacity = DownloadsSelected.Opacity = SettingsSelected.Opacity = StatisticsSelected.Opacity = 0;
            AboutSelected.Opacity = 1;
        }

        private void ScheduleSideBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetPage(new Schedule());
            HideSideBar();
            LibrarySelected.Opacity = DownloadsSelected.Opacity = AboutSelected.Opacity = SettingsSelected.Opacity = StatisticsSelected.Opacity = 0;
            ScheduleSelected.Opacity = 1;
        }

        private void LibrarySidebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetLibrary();
        }

        private void DownloadsSidebar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetPage(new DownloadsView());
            HideSideBar();
            LibrarySelected.Opacity = ScheduleSelected.Opacity = AboutSelected.Opacity = SettingsSelected.Opacity = StatisticsSelected.Opacity = 0;
            DownloadsSelected.Opacity = 1;
        }

        private void StatisticsSideBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            SetPage(new Statistics());
            HideSideBar();
            LibrarySelected.Opacity = ScheduleSelected.Opacity = AboutSelected.Opacity = SettingsSelected.Opacity = DownloadsSelected.Opacity = 0;
            StatisticsSelected.Opacity = 1;
        }

        private void SideButton_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void SideButton_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        public void SetLibrary() {
            SetPage(new Library());
            HideSideBar();
            LibrarySelected.Opacity = 1;
            DownloadsSelected.Opacity = ScheduleSelected.Opacity = AboutSelected.Opacity = SettingsSelected.Opacity  = StatisticsSelected.Opacity = 0;
        }

        #endregion

        public static bool CheckConnection() {
            Ping ping = new Ping();
            try {
                ping.Send("api.thetvdb.com");
                ping.Send("api.tvmaze.com");
                ping.Send("www.google.com");
                return true;
            } catch (PingException ex) {
                return false;
            }

        }

        private void SetDimensions() {
            if (Properties.Settings.Default.Maximized) {
                this.WindowState = WindowState.Maximized;
            } else {
                string height = Properties.Settings.Default.Resolution;
                List<string> dimensions = height.Split('x').ToList();
                Application.Current.MainWindow.Width = Int32.Parse(dimensions[0]);
                Application.Current.MainWindow.Height = Int32.Parse(dimensions[1]);
            }
        }

        /// <summary>
        /// Creates database from ids provided USAGE: await CreateDatabase(...)
        /// </summary>
        /// <param name="ids">Item1: TVDb ids, Item2: Path in library </param>     
        /// <returns></returns>
        public async static Task CreateDatabase(List<Tuple<int,string>> ids) {
            Window main = Application.Current.MainWindow;
            await ((MainWindow)main).GetFullShows(ids);
        }

        /// <summary>
        /// Creates database from ids provided USAGE: await CreateDatabase(...)
        /// </summary>
        /// <param name="ids">List of TVDb ids</param>     
        /// <returns></returns>
        private async Task GetFullShows(List<Tuple<int, string>> ids) {
            ProgressBarPage pbar = new ProgressBarPage(ids.Count);
            int total = 0;
            AddPage(pbar);
            //Wait for this task to complete without blocking main thread
            await Task.Run(() => {
                List<Task> tasks = new List<Task>();
                foreach (Tuple<int,string> combination in ids) {                                   
                    tasks.Add(Task.Run(() => {
                        int id = combination.Item1;
                        //Create 4 tasks, wait for them to complete and then set value of ProgressBar
                        Task[] secondTasks = new Task[4];
                        secondTasks[0] = Task.Run(() => {
                            Series s = Series.GetSeries(id);
                            s.libraryPath = combination.Item2;
                            Database.AddSeries(s);
                        });
                        secondTasks[1] = Task.Run(() => {
                            List<Actor> list = Actor.GetActors(id);
                            if (list != null) {
                                Database.AddActor(id, list);
                            } });
                        secondTasks[2] = Task.Run(() => {
                            List<Episode> list = Episode.GetEpisodes(id);
                            if (list != null) {
                                Database.AddEpisode(id, list);
                            }
                        });
                        secondTasks[3] = Task.Run(() => {
                            List<Poster> list = Poster.GetPosters(id);
                            if (list != null) {
                                Database.AddPoster(id, list);
                            }
                        });
                        Task.WaitAll(secondTasks);
                        Dispatcher.Invoke(new Action(() => {
                            total++;
                            pbar.SetValue(total);
                        }), DispatcherPriority.Send);
                    }));
                }
                //Wait for all tasks created in foreach to complete
                tasks.WaitAll();
                Thread.Sleep(500);
            });
            //This code runs after all API calls are done and stuff is saved
            PleaseWait pleaseWait = new PleaseWait();
            /*AddPage(pleaseWait);
            await Task.Run(() => {
                var list = Database.GetSeries().Where(x => ids.Any(y=>y.Item1 == x.id)).ToList();
                Renamer.FindAndRenameOptimized(list);
                foreach (int id in ids.Select(x=>x.Item1)) {
                    var series = Database.GetSeries(id);
                    Renamer.FindAndRename(series);
                }
                Thread.Sleep(500);
            });*/
            RemoveAllPages();
            SetPage(new Library());
        }

        private async void TestFunctions() {
            Stopwatch ne = new Stopwatch();
            var list = Database.GetSeries();
            ne.Start();
            await Renamer.ScanAndRename(Database.GetSeries()); 
            ne.Stop();
        }

        private void BaseGrid_Loaded(object sender, RoutedEventArgs e) {
            if (false) {
                NotificationSender.ShortCutCreator.TryCreateShortcut("TVSPlayer.app", "TVS-Player");
                if (!CheckConnection()) {
                    AddPage(new StartupInternetError());
                } else {
                    Settings.Load();
                    if (String.IsNullOrEmpty(Settings.Library)) {
                        AddPage(new Intro());
                        Helper.SetPerformanceMode();
                        Settings.LastCheck = DateTime.Now;
                        UpdateDatabase.StartUpdateBackground(false);
                    } else {
                        SetPage(new Library());
                        UpdateDatabase.StartUpdateBackground();
                        TorrentDownloader.ContinueUnfinished();
                    }
                }
            } else {
                Settings.Load();
                TestFunctions();
            }

        }
        
        /// <summary>
        /// Events that takes care of everything that needs to be saved before closing
        /// </summary>
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (dimensions != null) {
                if (dimensions.LastState == WindowState.Maximized) {
                    Properties.Settings.Default.Maximized = true;
                } else {
                    Properties.Settings.Default.Maximized = false;
                }
                Properties.Settings.Default.Resolution = dimensions.Width + "x" + dimensions.Height;
            } else {
                if (this.WindowState == WindowState.Maximized) {
                    Properties.Settings.Default.Maximized = true;
                } else {
                    Properties.Settings.Default.Maximized = false;
                }
                Properties.Settings.Default.Resolution = this.Width + "x" + this.Height;
            }
            Properties.Settings.Default.Save();

            if (videoPlayback) {
                try {
                    LocalPlayer player = (LocalPlayer)((Frame)ContentOnTop.Children[ContentOnTop.Children.Count - 1]).Content;
                    player.episode.continueAt = player.Player.MediaPosition - 50000000 > 0 ? player.Player.MediaPosition - 50000000 : 0;
                    player.episode.finised = player.Player.MediaDuration - 3000000000 < player.Player.MediaPosition ? true : false;
                    Database.EditEpisode(player.series.id, player.episode.id, player.episode);
                    player.Player.Stop();
                    player.Player.Close();
                } catch (Exception) {
                    TorrentStreamer player = (TorrentStreamer)((Frame)ContentOnTop.Children[ContentOnTop.Children.Count - 1]).Content;
                    var series = Database.GetSeries(player.downloader.TorrentSource.Series.id);
                    var ep = Database.GetEpisode(player.downloader.TorrentSource.Series.id, player.downloader.TorrentSource.Episode.id);
                    ep.continueAt = player.Player.MediaPosition - 50000000 > 0 ? player.Player.MediaPosition - 50000000 : 0;
                    ep.finised = player.Player.MediaDuration - 3000000000 < player.Player.MediaPosition ? true : false;
                    if (player.downloader.Status.IsSeeding) {
                        player.downloader.StopAndMove();
                    } else {
                        var torrent = player.downloader.TorrentSource;
                        torrent.IsSequential = false;
                        TorrentDatabase.Edit(torrent.Magnet, torrent);
                    }
                    Database.EditEpisode(series.id, ep.id, ep);

                    player.Player.Stop();
                    player.Player.Close();

                }

                //this will give the player some time to end properly
                Thread.Sleep(500);
            }
        }

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Process.Start("https://github.com/Kaharonus/TVS-Player/");
        }
    }

    

}