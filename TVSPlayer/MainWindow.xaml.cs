using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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



        #region Animations

        //Starts Storyboard animation of a grid
        private void StartAnimation(string storyboard, FrameworkElement grid) {
            Storyboard sb = this.FindResource(storyboard) as Storyboard;
            sb.Begin(grid);
        }

        //Shows side bar
        private void SideButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            HiderGrid.Visibility = Visibility.Visible;
            StartAnimation("ShowSideMenu", SideMenu);
            StartAnimation("OpacityUp", HiderGrid);
        }

        //Hides side bar
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
            BaseGrid.Children.Add(fr);
            Panel.SetZIndex(fr, 100);
            fr.Content = page;
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
        private void FinishedRemove(UIElement ue) {
            BaseGrid.Children.Remove(ue);
        }
        #endregion

        //Code for "Test" button
        private void Button_Click(object sender, RoutedEventArgs e) {

          
        }
    }

}