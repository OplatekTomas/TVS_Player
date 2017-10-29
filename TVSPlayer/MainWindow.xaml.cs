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
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TVS.API;

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
                StartAnimation("ShowNotification", RightButtons);
                NotificationArea.Visibility = Visibility.Visible;
                StartAnimation("OpacityUp", NotificationArea);
                rightsidevisible = true;

            }
        }

        #endregion

        #region Page handling

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
        public static void SetPage(Page page) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageSetter(page);
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
            if (!String.IsNullOrEmpty(custom.MainTitle)) {
                PageTitle.Text = custom.MainTitle;
            }
            if (custom.SearchBarEvent != null) {
                if (lastHandler != null) {
                    SearchBox.TextChanged -= lastHandler;
                }
                lastHandler = custom.SearchBarEvent;
                SearchBox.TextChanged += custom.SearchBarEvent;
            }
            if (custom.Buttons != null) {
                CustomContent.Children.RemoveRange(0, CustomContent.Children.Count);
                CustomContent.Children.Add(custom.Buttons);
            }
        }

        private void PageSetter(Page page) {
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
                PingReply tvdb = ping.Send("api.thetvdb.com");
                PingReply tvmaze = ping.Send("api.tvmaze.com");
                PingReply google = ping.Send("www.google.com");
                return true;
            } catch (PingException ex) {
                return false;
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
                        secondTasks[1] = Task.Run(() => Database.AddActor(id, Actor.GetActors(id)));
                        secondTasks[2] = Task.Run(() => Database.AddPoster(id, Poster.GetPosters(id)));
                        secondTasks[3] = Task.Run(() => Database.AddEpisode(id, Episode.GetAllEpisodes(id)));
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
            Series s = Series.GetSeries(121361);
            //Database.AddEpisode(121361,Episode.GetAllEpisodes(121361));
            s.libraryPath = @"D:\TVSTests\Game of Thrones";
            Settings.FirstScanLocation = @"";
            Settings.SecondScanLocation = @"";
            Settings.ThirdScanLocation = @"";


            //Settings.SaveSettings();
            Renamer.FindAndRename(s);
        }

        private void BaseGrid_Loaded(object sender, RoutedEventArgs e) {
            if (true) {
               
                if (!Directory.Exists(Helper.data)) {
                    AddPage(new Intro());
                } else {
                    SetPage(new Library());
                }
                if (!checkConnection()) {
                    AddPage(new StartupInternetError());
                }
                Settings.Load();

            } else {
                TestFunctions();
            }

        }

        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (this.WindowState == WindowState.Maximized) {
                Properties.Settings.Default.Maximized = true;
            } else {
                Properties.Settings.Default.Maximized = false;
            }
            Properties.Settings.Default.Resolution = this.Width + "x" + this.Height;
            Properties.Settings.Default.Save();
        }


    }

    

}