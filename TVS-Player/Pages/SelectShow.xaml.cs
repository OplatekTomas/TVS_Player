using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Threading;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for SelectShow.xaml
    /// </summary>
    public partial class SelectShow : Page{
        public SelectShow(){
            InitializeComponent();
        }
        string showNameTemp;

        private void nameTxt_TextChanged(object sender, TextChangedEventArgs e) {
            showNameTemp = nameTxt.Text;
            if (nameTxt.Text.Length >= 4 && nameTxt.Text != "Show name") {
                Action list;
                list = () => listShows();
                Thread thread = new Thread(list.Invoke);
                thread.Name = "List shows";
                thread.Start();
            }

        }
        struct Shows {
            public string specificInfo;
            public string showName;
            public DateTime date;
            public string id;
        }
        private void listShows() {
            string info = Api.apiGet(showNameTemp);
            int numberOfShows = 0;
            if (info != null) {
                JObject parse = JObject.Parse(info);
                numberOfShows = parse["data"].Count();
                Shows[] show = new Shows[numberOfShows];
                for (int i = 0; i < numberOfShows; i++) {
                    show[i].specificInfo = parse["data"][i].ToString();
                    show[i].showName = parse["data"][i]["seriesName"].ToString();
                    if (parse["data"][i]["firstAired"].ToString() == "") {
                        show[i].date = DateTime.ParseExact("1900-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture); ;
                    } else {
                        show[i].date = DateTime.ParseExact(parse["data"][i]["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                    show[i].id = parse["data"][i]["id"].ToString();
                }
                Array.Sort<Shows>(show, (x, y) => x.date.CompareTo(y.date));
                Array.Reverse(show);
                Dispatcher.Invoke(new Action(() => {
                    panel.Children.Clear();
                    for (int i = 0; i < numberOfShows; i++) {
                        addOption(show[i].showName, show[i].id, show[i].date.ToString("dd.MM.yyyy"), show[i].specificInfo);
                    }
                }), DispatcherPriority.Send);
            }

        }
        private void addOption(string showName, string id, string date, string specific) {
            tvShowControl option = new tvShowControl();
            option.showName.Text = showName;
            option.firstAir.Text = date;
            option.info.Click += (s, e) => { showInfo(specific); };
            option.confirm.Click += (s, e) => { selected(id, showName); };
            panel.Children.Add(option);

        }
        private void showInfo(string specific) {
            Page showPage = new ShowInfoSmall(specific);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
        }
        private void nameTxt_GotFocus(object sender, RoutedEventArgs e) {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= nameTxt_GotFocus;
        }
        private void selected(string id, string showName) {
            Helpers.showID = id;
            Helpers.showName = showName;
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
            //nameTxt.Text = string.Empty;
            //panel.Children.Clear();

        }
    }
}
