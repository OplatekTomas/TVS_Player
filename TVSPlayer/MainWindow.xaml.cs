using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        private bool theme;

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

        //Switches theme
        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ThemeSwitcher.SwitchTheme();
        }

        #endregion

        #region Page handling
        /// <summary>
        /// Function that renders new frame and page on top of existing content
        /// </summary>
        /// <param name="page">Page you want to show</param>
        public static void AddPage(Page page) {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageCreator(page);
            
        }
        //Adds new page
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
        /// <summary>
        /// Removes last page added
        /// </summary>
        public static void RemovePage() {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageRemover();
        }
        //Removes last page
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
        /// <summary>
        /// Call this function (and this function only) when you need to search API (returns either basic info about Series or null)
        /// </summary>
       public async Task<Series> SearchShowAsync() {
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
        #endregion

        //Code for "Test" button
        private void Button_Click(object sender, RoutedEventArgs e) {

          
        }

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
        public async Task GetFullShows(List<Tuple<int, string>> ids) {
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
            });
            //This code runs after all API calls are done and stuff is saved
            RemovePage();
        }

        private async void TestFunctions() {
            Series s = Series.GetSeries(121361);
            //Database.AddEpisode(121361,Episode.GetAllEpisodes(121361));
            s.libraryPath = @"C:\Users\tomas\TVSTests\Game of Thrones";
            Settings.FirstScanLocation = @"C:\Users\tomas\TVSTests\FirstTest";
            Settings.SecondScanLocation = @"";
            Settings.ThirdScanLocation = @"";


            //Settings.SaveSettings();
            Renamer.FindAndRename(s);
        }

        private void BaseGrid_Loaded(object sender, RoutedEventArgs e) {
            Settings.Load();
            if (false) {
                if (!Directory.Exists(Helper.data)) {
                    //AddPage(new Intro());
                } else {

                }

                if (!checkConnection()) {
                    AddPage(new StartupInternetError());
                }
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
            Properties.Settings.Default.Resolution = Math.Ceiling(this.ActualWidth) + "x" + Math.Ceiling(this.ActualHeight);
            Properties.Settings.Default.Save();
        }
    }

}