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
using Newtonsoft.Json;
using Ragnar;

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
            StartUp();
        }
        Thread CheckThread;
        Timer t = new Timer();
        public static volatile List<Notification> notifications = new List<Notification>();
        public static HashSet<SearchItem> searchIndex = new HashSet<SearchItem>();
        HashSet<Tuple<string, SearchItem>> searchTuple = new HashSet<Tuple<string, SearchItem>>();
        public static List<Tuple<TorrentHandle, Notification>> torrents = new List<Tuple<TorrentHandle, Notification>>();

        private bool Check() {
            if (DatabaseShows.ReadDb().Count == 0) {
                return true;
            }
            return false;
        }

        private void StartUp() {
            if (!Check()) {
                //RunChecker();
                DownloadCheckTimer();
            } else {
                Page p = new Startup();
                AddTempFrameIndex(p);
            }
        }

        private void DownloadCheckTimer() {
            Timer t = new Timer();
            t.Interval = 1000;
            Notification n = new Notification();
            t.Elapsed += (s, e) => DownloadCheckNotifications();
            t.Start();
        }
        private void DownloadCheckNotifications() {
            for (int i = 0; i < torrents.Count;i++) {
                if (torrents[i].Item2 == null) {
                    Dispatcher.Invoke(new Action(() => {
                        Notification n = new Notification();
                        torrents[i] = new Tuple<TorrentHandle, Notification>(torrents[i].Item1, n);
                        notifications.Add(n);
                    }), DispatcherPriority.Send);
                }
            }
            DownloadCheck();
        }
        private void DownloadCheck() {
            foreach (Tuple < TorrentHandle, Notification > tuple in torrents) {
                TorrentHandle handle = tuple.Item1;
                Notification notification = tuple.Item2;
                var status = handle.QueryStatus();
                if (status.IsSeeding) {
                    break;
                }
                int speed = status.DownloadRate;
                Dispatcher.Invoke(new Action(() => {
                    try {
                        notification.MainText.Text = handle.TorrentFile.Name;
                    } catch (Exception) {
                        notification.MainText.Text = "Getting metadata";
                    }
                    notification.SecondText.Text = GetSpeed(speed);
                    notification.ProgBar.Value = status.Progress * 100;
                    int index = MainWindow.notifications.IndexOf(notification);
                    MainWindow.notifications[index] = notification;
                }), DispatcherPriority.Send);
            }
        }
        private string GetSpeed(float speed) {
            string speedText = speed + " B/s";
            if (speed > 1000) {
                speedText = (speed / 1000).ToString("N0") + " kB/s";
            }
            if (speed > 1000000) {
                speedText = (speed / 1000000).ToString("N1") + " MB/s";
            }
            if (speed > 1000000000) {
                speedText = (speed / 1000000000).ToString("N1") + " GB/s";
            }
            return speedText;
        }


        private void RunChecker() {
            Notification n = new Notification();
            Action a = () => CheckChanges(n);
            CheckThread = new Thread(a.Invoke);
            CheckThread.Name = "Background checker for changes";
            CheckThread.IsBackground = true;
            CheckThread.Start();
        }
        private void CheckChanges(Notification n) {
            while (true) {
                if (LastLaunch < (DateTime.Now.AddDays(-1))) {
                    Task t1 = new Task(() => UpdateDBAll(n));
                    var secondtask = t1.ContinueWith(t3 => SaveSearch());
                    secondtask.ContinueWith(t4 => LoadSearch());
                    t1.Start();
                }
                Task t2 = new Task(() => LoadSearch());
                t2.ContinueWith(t => RescanFilesAll(n));
                t2.Start();
                Thread.Sleep(TimeSpan.FromHours(1));
            }
        }

        public void SaveSearch() {          
            SearchIndexDB.SaveDB(CreateSearchIndex());
        }
        public void LoadSearch() {
            searchIndex = new HashSet<SearchItem>(SearchIndexDB.ReadDB());
            searchTuple = MergeName();
        }

        private List<SearchItem> CreateSearchIndex() {
            List<SearchItem> items = new List<SearchItem>();
            foreach (Show s in DatabaseShows.ReadDb()) {
                foreach (Episode ep in DatabaseEpisodes.ReadDb(s.id)) {
                    items.Add(new SearchItem(ep,s));
                }
            }
            items.Reverse();
            return items;
        }

        private void RescanFilesAll(Notification n) {
            List<Show> sh = DatabaseShows.ReadDb();
            Dispatcher.Invoke(new Action(() => {
                n = new Notification();
                n.ProgBar.Maximum = sh.Count;
                notifications.Add(n);
            }), DispatcherPriority.Send);
            foreach (Show s in sh) {
                Dispatcher.Invoke(new Action(() => {
                    int sIndex = sh.IndexOf(s) + 1;
                    n.MainText.Text = "Updating files: " + s.name;
                    n.SecondText.Text = sIndex + "/" + sh.Count;
                    n.ProgBar.Value = sIndex;
                    int index = notifications.IndexOf(n);
                    notifications[index] = n;
                }), DispatcherPriority.Send);
                Checker.RescanEP(s.id);
            }
            notifications.Remove(n);
            Dispatcher.Invoke(new Action(() => {
                NotificationsList.Children.Remove(n);
            }), DispatcherPriority.Send);
        }
        private void UpdateDBAll(Notification n) {
            List<Show> sh = DatabaseShows.ReadDb();
            Dispatcher.Invoke(new Action(() => {
                n = new Notification();
                n.ProgBar.Maximum = sh.Count;
                notifications.Add(n);
            }), DispatcherPriority.Send);
            foreach (Show s in sh) {
                Dispatcher.Invoke(new Action(() => {
                    int sIndex = sh.IndexOf(s) + 1;
                    n.MainText.Text = "Updating DB: " + s.name;
                    n.SecondText.Text = sIndex + "/" + sh.Count;
                    n.ProgBar.Value = sIndex;
                    int index = notifications.IndexOf(n);
                    notifications[index] = n;
                }), DispatcherPriority.Send);
                Checker.Update(s.id);
            }
            notifications.Remove(n);
            Dispatcher.Invoke(new Action(() => {
                NotificationsList.Children.Remove(n);
            }), DispatcherPriority.Send);
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

        private void FrameLoaded_Handler(object sender, RoutedEventArgs e) {
            SearchBar.TextChanged += SearchBar_TextChanged;
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
            Frame frameOnTop = new Frame();
            frameOnTop.Content = page;
            GridOnTop.Children.Add(frameOnTop);
            Panel.SetZIndex(frameOnTop, 1000);

        }

        public void CloseTempFrame() {
            BaseGrid.Children.RemoveAt(BaseGrid.Children.Count - 1);
        }
        public void CloseTempFrameIndex() {
            GridOnTop.Children.RemoveAt(GridOnTop.Children.Count - 1);
        }
        public void SetTitle(string text) {
            HeadingText.Text = text;
        }

        private void SearchBar_GotFocus(object sender, RoutedEventArgs e) {
            SearchBar.Text = "";
            SearchBar.GotFocus -= SearchBar_GotFocus;
        }

        private void ShowLibraryButon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Library)) {
                Frame.Content = new Library();
            }
        }

        private void DownloadPageButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Download)) {
                //Api.getToken();
                //Api.apiGetPoster(73871,"Futurama");
                //string kappa = Api.apiGet(1,1, 73871);
                Frame.Content = new Startup();

                //string test = AppSettings.GetLibLocation();
                //Checker.UpdateShowFull(277928);
                //Checker.CheckForUpdates(121361);
            }
        }

        public void SetHeading(string text) {
            HeadingText.Text = text;
        }

        private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(Settings)) {
                Frame.Content = new Settings();
            }
        }

        private void InfoButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            HideMenu();
            if (Frame.Content.GetType() != typeof(About)) {
                Frame.Content = new About();
            }
        }

        public static Notification AddNotification(string MainText, string SecondaryText) {
            Notification not = new Notification();
            not.MainText.Text = MainText;
            not.SecondText.Text = SecondaryText;
            notifications.Add(not);
            return not;
        }
        public static void EditNotification(Notification n,string MainText, string SecondaryText,int ProgressBarValue) {
            int index = notifications.IndexOf(n);
            notifications[index].MainText.Text = MainText;
            notifications[index].SecondText.Text = SecondaryText;
            notifications[index].ProgBar.Value = ProgressBarValue;
        }
        int prev = 0;
        private void RenderNotifications() {
            for (int i = 0; i < notifications.Count; i++) {
                if (prev > notifications.Count) {
                    Dispatcher.Invoke(new Action(() => {
                        NotificationsList.Children.RemoveRange(0, NotificationsList.Children.Count);
                    }), DispatcherPriority.Send);
                }
                prev = notifications.Count;
                Dispatcher.Invoke(new Action(() => {
                    if (NotificationsList.Children.Count < notifications.Count) {
                    while (NotificationsList.Children.Count < notifications.Count) {
                        Notification n = new Notification();
                        NotificationsList.Children.Add(n);
                    }
                }
                    NotificationsList.Children.RemoveAt(i);
                    NotificationsList.Children.Insert(i, notifications[i]);
                }), DispatcherPriority.Send);
            }
        }

        private void NotificationButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (notifications.Count > 0) {
                prev = notifications.Count;
                t.Interval = 200;
                t.Elapsed += (s, ea) => RenderNotifications();
                t.Enabled = true;
                NotificationPanel.Visibility = Visibility.Visible;
                NotificationPanel.Focusable = true;
                NotificationPanel.Focus();
            } else {
                MessageBox.Show("No notifications", "Info");
            }
        }
        private void NotificationPanel_LostFocus(object sender, RoutedEventArgs e) {
            t.Enabled = false;
            NotificationsList.Children.RemoveRange(0, NotificationsList.Children.Count);
            NotificationPanel.Focusable = false;
            NotificationPanel.Visibility = Visibility.Hidden;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e) {
            SearchList.Children.Clear();
            if (SearchBar.Text.Length > 1) {
                string text = SearchBar.Text.ToUpper();
                List<SearchItem> ItemList = new List<SearchItem>();
                foreach (Tuple<string, SearchItem> t in searchTuple) {
                    if (t.Item1.Contains(text)) {
                        ItemList.Add(t.Item2);
                    }
                }
                if (ItemList.Count > 0) {
                    SearchPanel.Visibility = Visibility.Visible;
                    foreach (SearchItem s in ItemList) {
                        SearchResult se = new SearchResult();
                        se.MouseLeftButtonDown += (s1,e1) => LoadEP(s);
                        se.SerachText.Text = s.show.name + " " + s.seasonNumber + s.episodeNumber + " " + s.episodeObject.name;
                        SearchList.Children.Add(se);
                    }
                }
            }        
        }

        private void LoadEP(SearchItem s) {
            SetFrameView(new Episodes(s.show,s.episodeObject.season,s.episodeObject));
        }

        private HashSet<Tuple<string, SearchItem>> MergeName() {
            HashSet<Tuple<string, SearchItem>> list = new HashSet<Tuple<string, SearchItem>>();
            foreach (SearchItem i in searchIndex) {
                list.Add(new Tuple<string, SearchItem>(i.show.name.ToUpper() + " " + i.seasonNumber.ToUpper() + i.episodeNumber.ToUpper() + " " + i.episodeObject.name.ToUpper(), i));
            }
            return list;
        }

        private void SearchBar_LostFocus(object sender, RoutedEventArgs e) {
            SearchPanel.Visibility = Visibility.Hidden;
        }
    }   
}


