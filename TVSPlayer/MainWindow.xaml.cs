using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TVSPlayer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

        }
        private bool theme;


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
        /// Call this function (and this function only) when you need to search API (returns either basic info about TV Show or null)
        /// </summary>
       /* public async Task<TVShow> SearchShowAsync() {
            Page page = new SearchAPIPage();
            Frame fr = new Frame();
            fr.Opacity = 0;
            StartAnimation("OpacityUp", fr);
            Grid.SetRowSpan(fr, 2);
            BaseGrid.Children.Add(fr);
            Panel.SetZIndex(fr, 1000);
            fr.Content = page;
            TVShow s = await Helper.ReturnTVShowWhenNotNull();
            RemovePage();
            Helper.show = null;
            if (s == new TVShow()) {
                return null;
            }
            return s;
        }*/
        // Event that is called after animation of removing page is done - actualy removes the page
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
        private void BaseGrid_Loaded(object sender, RoutedEventArgs e) {
            if (!Directory.Exists(Helper.data)) {
                AddPage(new Intro());
            } else {
                
            }

            if (!checkConnection()) {
                AddPage(new StartupInternetError());
            }
        }
    }

}