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

        //Removes default text from search bar and starts search after text in search bar has changed
        private void LocationBox_GotFocus(object sender, RoutedEventArgs e) {
            LocationBox.GotFocus -= LocationBox_GotFocus;
            LocationBox.Text = "";
            LocationBox.TextChanged += (s, ea) => StartSearch();
        }

        //Starts search as a new thread
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

        //Does actual search in a new thread - parameter is search bar text
        private void Search(string text) {
            List<TVShow> shows = TVShow.Search(text);
            foreach (TVShow show in shows) {
                Dispatcher.Invoke(new Action(() => {
                    ShowSearchResult sh = new ShowSearchResult();
                    sh.Height = 55;
                    sh.ShowName.Text = show.seriesName;
                    sh.ConfirmButton.MouseUp += (s,e) => Confirm(show);
                    sh.DetailsButton.MouseUp += (s, e) => Details(show);
                    ShowList.Children.Add(sh);
                }));
            }
        }

        //Opens Page with details about specific TV Show
        private void Details(TVShow s) {
            MainWindow.AddPage(new Details(s));
        }

        //Confirms and "returns" selected tv show
        private void Confirm(TVShow s) {
            Helper.show = s;
        }

        //Closes this page
        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Helper.show = new TVShow();
        }
    }
}
