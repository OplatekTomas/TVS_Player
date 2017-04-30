using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        private void StartAnimation(string storyboard, Grid grid) {
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
        private void Button_Click(object sender, RoutedEventArgs e) {
            /*Stopwatch sw = new Stopwatch();
            sw.Start();
            List<TVShow> s = TVShow.Search("Lost");
            sw.Stop();
            string test = sw.ElapsedMilliseconds + "\n";
            foreach (TVShow show in s) {
                test += show.seriesName + "\n";
            }
            MessageBox.Show(test);
            s[0].GetInfo();
            MessageBox.Show(s[0].overview);*/
            Page p = new ImportScanFolder();
            AddPage(p);
        }

        public void AddPage(Page page) {
            Frame fr = new Frame();
            Grid.SetRowSpan(fr , 2);
            BaseGrid.Children.Add(fr);           
            Panel.SetZIndex(fr , 1000);
            fr.Content = page;
        }
        public void RemovePage() {
            var p = BaseGrid.Children[BaseGrid.Children.Count - 1] as Frame;
            Storyboard sb = this.FindResource("OpacityDown") as Storyboard;
            Storyboard sbLoad = sb.Clone();
            sbLoad.Completed += (s, e) => FinishedRemove();
            sbLoad.Begin(p);
        }
        public void FinishedRemove() {
            BaseGrid.Children.RemoveAt(BaseGrid.Children.Count - 1);
        }

    }

}