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
using Timer = System.Timers.Timer;

namespace TVSPlayer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window {

        public MainWindow() {
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
        private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
            StartAnimation("HideSearch", SearchBar);
            StartAnimation("OpacityDown", SearchBar);
            StartAnimation("MoveSearchRight", SearchButton);
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

        public static void GoFullScreen(bool reset = false) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).FullscreenSetter(reset);
        }
        bool isFullscreen = false;
        WindowState lastState;
        public void FullscreenSetter(bool reset) {
            if (reset) {
                if (this.WindowStyle == WindowStyle.None) {
                    this.WindowState = lastState;
                    isFullscreen = false;
                    this.Topmost = false;
                    this.ResizeMode = ResizeMode.CanResize;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                }
            } else if (!isFullscreen) {
                lastState = this.WindowState;
                isFullscreen = true;
                this.Visibility = Visibility.Collapsed;
                this.Topmost = true;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
                this.Visibility = Visibility.Visible;
            } else {
                this.WindowState = lastState;
                isFullscreen = false;
                this.Topmost = false;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }

        }

        public static string GetSearchBarText() {
            Window main = Application.Current.MainWindow;
            return ((MainWindow)main).SearchBox.Text;
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

        /// <summary>
        /// Sets page in Main Frame to any page
        /// </summary>
        /// <param name="page">What page to set to</param>
        public static void SetPage(FrameworkElement page) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageSetter(page);
            SetPageCustomization(new PageCustomization());
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
                lastHandler = custom.SearchBarEvent;
                SearchBox.TextChanged += custom.SearchBarEvent;
            }
            if (custom.Buttons != null) {
                CustomContent.Children.RemoveRange(0, CustomContent.Children.Count);
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
            Panel.SetZIndex(fr, 100);
            fr.Content = page;
            Storyboard sb = this.FindResource("OpacityUp") as Storyboard;
            Storyboard sbLoad = sb.Clone();
            BaseGrid.Children.Add(fr);
            sbLoad.Begin(fr);
        }
        
        private void PageRemover() {
            var p = BaseGrid.Children[BaseGrid.Children.Count - 1] as FrameworkElement;
            Storyboard sb = this.FindResource("OpacityDown") as Storyboard;
            Storyboard sbLoad = sb.Clone();
            sbLoad.Completed += (s, e) => FinishedRemove(p);
            sbLoad.Begin(p);
        }

        private void FinishedRemove(UIElement ue) {
            BaseGrid.Children.Remove(ue);
        }

        #endregion

        #region Events

        private void ThemeSwitch_MouseUp(object sender, MouseButtonEventArgs e) {
            ThemeSwitcher.SwitchTheme();
        }
        #endregion



        public static bool checkConnection() {
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

        private void StartDailyUpdate() {
            if (Settings.LastCheck.AddDays(1) < DateTime.Now) {
                //UpdateDatabase();
                Settings.LastCheck = DateTime.Now;
            }
            Timer timer = new Timer(86400000);
            timer.Elapsed += (s, ev) => UpdateDatabase();
            //timer.Start();
        }

        private async void UpdateDatabase() {
            await Task.Run(() => {
                List<Series> series = Database.GetSeries();           
                List<int> ids = Series.GetUpdates(Settings.LastCheck);
                List<Series> toUpdate = new List<Series>();
                foreach (var ser in series) {
                    foreach (int id in ids) {
                        if (ser.id == id) {
                            toUpdate.Add(ser);
                            break;
                        }
                    }
                }
                foreach (var ser in toUpdate) {
                    UpdateSeries(ser);
                }
            });
            Settings.LastCheck = DateTime.Now;
        }

        private void UpdateSeries(Series series) {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => {
            }));
            tasks.Add(Task.Run(() => {
            }));
            tasks.Add(Task.Run(() => {
            }));
            tasks.Add(Task.Run(() => {
            }));
            tasks.WaitAll();
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
            ProgressBarPage prog = new ProgressBarPage(ids.Count);
            AddPage(prog);
            total = 0;
            await Task.Run(() => {
                List<Series> seriesList = Database.GetSeries();
                foreach (Series series in seriesList) {
                    Renamer.FindAndRename(series);
                    Dispatcher.Invoke(new Action(() => {
                        total++;
                        prog.SetValue(total);
                    }), DispatcherPriority.Send);
                }
                Thread.Sleep(1000);
            });
            SetPage(new Library());
        }

        private async void TestFunctions() {
            SetPage(new ActorUserControl(null) { Height=300 });

        }

        private void BaseGrid_Loaded(object sender, RoutedEventArgs e) {
            if (true) {
                if (!Directory.Exists(Helper.data)) {
                    AddPage(new Intro());
                    Settings.LastCheck = DateTime.Now;
                } else {
                    SetPage(new Library());
                    Settings.Load();

                    StartDailyUpdate();
                }
                if (!checkConnection()) {
                    AddPage(new StartupInternetError());
                }


            } else {
                TestFunctions();
            }

        }

        private async void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (this.WindowState == WindowState.Maximized) {
                Properties.Settings.Default.Maximized = true;
            } else {
                Properties.Settings.Default.Maximized = false;
            }
            Properties.Settings.Default.Resolution = this.Width + "x" + this.Height;
            Properties.Settings.Default.Save();
            if (videoPlayback) {
                LocalPlayer player = (LocalPlayer)((Frame)BaseGrid.Children[BaseGrid.Children.Count - 1]).Content;
                player.Player.Stop();
                player.Player.Close();
                //this will give the player some time to end properly
                Thread.Sleep(500);
            }
        }


    }

    

}