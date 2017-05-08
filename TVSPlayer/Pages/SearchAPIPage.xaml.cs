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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SearchAPIPage.xaml
    /// </summary>
    public partial class SearchAPIPage : Page {
        public SearchAPIPage() {
            InitializeComponent();
        }
        private void LocationBox_GotFocus(object sender, RoutedEventArgs e) {
            LocationBox.GotFocus -= LocationBox_GotFocus;
            LocationBox.Text = "";
            LocationBox.TextChanged += (s, ea) => StartSearch();
        }
        Thread searchThread;
        private void StartSearch() {
            ShowList.Children.Clear();
            if (searchThread != null) {
                searchThread.Abort();
            }
            string text = LocationBox.Text;
            Action a = () => Search(text);
            searchThread = new Thread(a.Invoke);
            searchThread.IsBackground = true;
            searchThread.Name = "API Search";
            searchThread.Start();
        }
        private void Search(string text) {
            List<TVShow> shows = TVShow.Search(text);
            foreach (TVShow show in shows) {
                Dispatcher.Invoke(new Action(() => {
                    ShowSearchResult sh = new ShowSearchResult();
                    sh.Height = 45;
                    sh.ShowName.Text = show.seriesName;
                    sh.ConfirmButton.MouseUp += (s,e) => Confirm(show);
                    ShowList.Children.Add(sh);
                }));
            }
        }
        private void Confirm(TVShow s) {
            Helper.show = s;
        }

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Helper.show = new TVShow();
        }
    }
}
