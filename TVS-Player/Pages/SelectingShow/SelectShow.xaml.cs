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
            public string showName;
            public DateTime date;
            public string id;
            public string status;
        }
        private void listShows() {
            string info = Api.apiGet(showNameTemp);
            int numberOfShows = 0;
            if (info != null) {
                JObject parse = JObject.Parse(info);
                numberOfShows = parse["data"].Count();
                Shows[] show = new Shows[numberOfShows];
                for (int i = 0; i < numberOfShows; i++) {
                    show[i].showName = parse["data"][i]["seriesName"].ToString();
                    if (parse["data"][i]["firstAired"].ToString() == "") {
                        show[i].date = DateTime.ParseExact("1900-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture); ;
                    } else {
                        show[i].date = DateTime.ParseExact(parse["data"][i]["firstAired"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                    show[i].id = parse["data"][i]["id"].ToString();
                    show[i].status = parse["data"][i]["status"].ToString();
                }
                Array.Sort<Shows>(show, (x, y) => x.date.CompareTo(y.date));
                Array.Reverse(show);
                Dispatcher.Invoke(new Action(() => {
                    panel.Children.Clear();
                    for (int i = 0; i < numberOfShows; i++) {
                        addOption(new Show(Int32.Parse(show[i].id), show[i].showName, show[i].status), show[i].date.ToString("dd.MM.yyyy"));
                    }
                }), DispatcherPriority.Send);
            }

        }
        private void addOption(Show sh, string date) {
            tvShowControl option = new tvShowControl();
            option.showName.Text = sh.name;
            option.firstAir.Text = date;
            option.info.Click += (s, e) => { showInfo(sh.id); };
            option.confirm.Click += (s, e) => { selected(sh); };
            panel.Children.Add(option);

        }
        private void showInfo(int id) {
            Page showPage = new ShowInfoSmall(Api.apiGet(id));
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrameIndex(showPage);
        }
        private void nameTxt_GotFocus(object sender, RoutedEventArgs e) {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= nameTxt_GotFocus;
        }
        private void selected(Show show) {
            Helpers.show = show;
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
            //nameTxt.Text = string.Empty;
            //panel.Children.Clear();

        }
    }
}
