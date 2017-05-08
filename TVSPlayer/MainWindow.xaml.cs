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
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

        }

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

        //Code for "Test" button
        private async void Button_Click(object sender, RoutedEventArgs e) {
            TVShow s = new TVShow();
            s.tvmazeId = 82;
            s.GetInfoTVMaze();
            //Page p = new ImportScanFolder();
            //AddPage(p);
            //await SearchShowAsync();
        }

        //Simple functoin that render new frame and page on top of existing content
        public void AddPage(Page page) {
            Frame fr = new Frame();
            Grid.SetRowSpan(fr , 2);
            BaseGrid.Children.Add(fr);           
            Panel.SetZIndex(fr , 1000);
            fr.Content = page;
        }

        //Call this function (and this function only) when you need to search API (returns either basic info about TV Show or null)
        public async Task<TVShow> SearchShowAsync() {
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
        }

        //Removes last page added
        public void RemovePage() {
            var p = BaseGrid.Children[BaseGrid.Children.Count - 1] as Frame;
            Storyboard sb = this.FindResource("OpacityDown") as Storyboard;
            Storyboard sbLoad = sb.Clone();
            sbLoad.Completed += (s, e) => FinishedRemove();
            sbLoad.Begin(p);
        }
        // Event that is called after animation of removing page is done - actualy removes the page
        private void FinishedRemove() {
            BaseGrid.Children.RemoveAt(BaseGrid.Children.Count - 1);
        }

    }

}