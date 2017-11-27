using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVS.API;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SearchSingleShow.xaml
    /// </summary>
    public partial class SearchSingleShow : Page {
        public SearchSingleShow() {
            InitializeComponent();
        }

        public Series show = null;
        /// <summary>
        /// Asynchronous task that waits until variable show is not null and then returns this variable
        /// </summary>
        public async Task<Series> ReturnTVShowWhenNotNull() {
            Series s = null;
            await Task.Run(() => {
                do {
                    s = show;
                    Thread.Sleep(100);
                } while (show == null);
                s = show;
            });
            show = null;
            return s;
        }

        private void SelectFolderText_GotFocus(object sender, RoutedEventArgs e) {
            SelectFolderText.Text = "";
            SelectFolderText.TextAlignment = TextAlignment.Left;
            SelectFolderText.GotFocus -= SelectFolderText_GotFocus;
            SelectFolderText.TextChanged += SelectFolderText_TextChanged;
        }

        Task<List<Series>> task;
        private void SelectFolderText_TextChanged(object sender, TextChangedEventArgs e) {
            string name = SelectFolderText.Text;
            if (task != null && task.Status != TaskStatus.RanToCompletion && task.Status == TaskStatus.Running) {
                task.ContinueWith((t) => { });
            }
            task = new Task<List<Series>>(() => Series.Search(name));
            task.ContinueWith((t) => {
                FillUI(task.Result);
            });
            task.Start();
        }
        private void FillUI(List<Series> list) {
            ClearList();
            if (list != null) { 
                Task.Run(() => {
                    foreach (Series sh in list) {
                        Dispatcher.Invoke(new Action(() => {
                            SearchShowResult sr = new SearchShowResult(sh.id);
                            sr.Height = 50;
                            sr.Opacity = 0;
                            Storyboard MoveUp = FindResource("OpacityUp") as Storyboard;
                            MoveUp.Begin(sr);
                            sr.SeriesName.Text = sh.seriesName;
                            sr.Confirm.MouseLeftButtonUp += (se, e) => {
                                show = sh; };
                            panel.Children.Add(sr);
                        }), DispatcherPriority.Send);
                        Thread.Sleep(7);
                    }

                });
            }
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e) {
            show = new Series();
        }
        private void ClearList() {
            Dispatcher.Invoke(new Action(() => {
                UIElementCollection children = panel.Children;
                foreach (SearchShowResult child in children) {
                    Storyboard OpacityDown = FindResource("OpacityDown") as Storyboard;
                    OpacityDown.Begin(child);
                    Thread.Sleep(1);
                }
                panel.Children.Clear();

            }), DispatcherPriority.Send);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
            Series s = new Series();
            s.imdbId = "kua";
            show = s;
        }

        private void BackButton_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void BackButton_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }
    }
}
