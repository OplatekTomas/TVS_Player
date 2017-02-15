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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.IO;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Windows.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        DateTime LastLaunch;
        public MainWindow() {
            InitializeComponent();
            Api.getToken();
            LastLaunch = Properties.Settings.Default.LastLaunched;
            Properties.Settings.Default.LastLaunched = DateTime.Now;
            Properties.Settings.Default.Save();
            RunChecker();
        }
        Thread CheckThread;
        Timer t = new Timer();
        public static List<Notification> notifications = new List<Notification>();


        private void RunChecker() {
            Notification n = new Notification();
            Action a = () => CheckChanges(n);
            CheckThread = new Thread(a.Invoke);
            CheckThread.Name = "Background checker for changes";
            CheckThread.Start();
        }
        private void CheckChanges(Notification n) {
            while (true) {
                if (LastLaunch < (DateTime.Now.AddDays(-1))) {
                }
                UpdateDBAll(n);
                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }

        private void UpdateDBAll(Notification n) {
            List<Show> sh = DatabaseShows.ReadDb();
            Dispatcher.Invoke(new Action(() => {
                n.ProgBar.Maximum = sh.Count;
                notifications.Add(n);
            }), DispatcherPriority.Send);
            foreach (Show s in sh) {
                Dispatcher.Invoke(new Action(() => {
                    int sIndex = sh.IndexOf(s) + 1;
                    n.MainText.Text = "Updating: " + s.name;
                    n.SecondText.Text = sIndex + "/" + sh.Count;
                    n.ProgBar.Value = sIndex;
                    int index = MainWindow.notifications.IndexOf(n);
                    notifications[index] = n;
                }), DispatcherPriority.Send);
                Checker.UpdateFull(s.id);
            }
            MainWindow.notifications.Remove(n);

        }

        private void MenuButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            ShowHideMenu("sbShowLeftMenu", Sidebar);
            MenuBackground.Visibility = Visibility.Visible;
            ShowHideMenu("sbShowBackground", MenuBackground);
        }

        private void MenuHideButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
        }

        private void HideMenu() {
            ShowHideMenu("sbHideLeftMenu", Sidebar);
            MenuBackground.Visibility = Visibility.Hidden;
            ShowHideMenu("sbHideBackground", MenuBackground);
        }

        private void ShowHideMenu(string Storyboard, Grid pnl) {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);
        }
        /* private void btnShowsShow_Click(object sender, RoutedEventArgs e) {

         }
         private void btnDownloadShow_Click(object sender, RoutedEventArgs e){
          
         }*/

        private void FrameLoaded_Handler(object sender, RoutedEventArgs e) {
            Api.getToken();
        }

        public void SetFrameView(Page page) {
            if (Frame.Content.GetHashCode() != page.GetHashCode()) {
                Frame.Content = page;
            }
        }
        public void AddTempFrame(Page page) {
            Frame fr = new Frame();
            BaseGrid.Children.Add(fr);
            Panel.SetZIndex(fr, 1000);
            fr.Content = page;
        }
        public void AddTempFrameIndex(Page page) {
            Frame fr = new Frame();
            GridOnTop.Children.Add(fr);
            Panel.SetZIndex(fr, 1000);
            fr.Content = page;
        }

        public void CloseTempFrame() {
            BaseGrid.Children.RemoveAt(BaseGrid.Children.Count - 1);
        }
        public void CloseTempFrameIndex() {
            GridOnTop.Children.RemoveAt(GridOnTop.Children.Count - 1);
        }

        private void SearchBar_GotFocus(object sender, RoutedEventArgs e) {
            SearchBar.Text = "";
            SearchBar.GotFocus -= SearchBar_GotFocus;
        }

        private void ShowLibraryButon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Shows)) {
                Frame.Content = new Shows();
            }
        }

        private void DownloadPageButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Download)) {
                Api.getToken();
                //Api.apiGetPoster(73871,"Futurama");
                //string kappa = Api.apiGet(1,1, 73871);
                //Frame.Content = new Startup();

                //string test = AppSettings.GetLibLocation();
                //Checker.UpdateShowFull(277928);
                //Checker.CheckForUpdates(121361);
            }
        }

        public void SetHeading(string text) {
            HeadingText.Text = text;
        }

        private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void InfoButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        }

        private void RenderNotifications() {
            for (int i = 0; i < notifications.Count; i++) {
                Dispatcher.Invoke(new Action(() => {
                    NotificationsList.Children[i] = notifications[i];
                }), DispatcherPriority.Send);
            }
        }

        private void NotificationButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (notifications.Count > 0) {
                foreach (Notification n in notifications) {
                    NotificationsList.Children.Add(n);
                }
                t.Interval = 100;
                t.Elapsed += (s, ea) => RenderNotifications();
                t.Enabled = true;
                NotificationPanel.Visibility = Visibility.Visible;
                NotificationPanel.Focusable = true;
                NotificationPanel.Focus();
            }
        }
        private void NotificationPanel_LostFocus(object sender, RoutedEventArgs e) {
            t.Enabled = false;
            NotificationsList.Children.RemoveRange(0, NotificationsList.Children.Count);
            NotificationPanel.Focusable = false;
            NotificationPanel.Visibility = Visibility.Hidden;
        }
    }
}
