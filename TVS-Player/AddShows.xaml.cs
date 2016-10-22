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
using System.Windows.Forms;
using System.IO;
using Ookii.Dialogs.Wpf;
using TextBox = System.Windows.Controls.TextBox;
using Path = System.IO.Path;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Newtonsoft.Json;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for AddShows.xaml
    /// </summary>
    public partial class AddShows : Window {
        public AddShows() {
            InitializeComponent();
        }
        string path;
        string showNameTemp;
        private void nameTxt_GotFocus(object sender, RoutedEventArgs e) {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= nameTxt_GotFocus;
        }

        private void showLocation_GotFocus(object sender, RoutedEventArgs e) {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= showLocation_GotFocus;
        }

        private void selectFolder(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog ofd = new VistaFolderBrowserDialog();
            var check = ofd.ShowDialog();
            if (check == true) {
                path = ofd.SelectedPath;
                showLocation.Text = path;
            }
            if (Path.GetFileName(path) != "Downloads") {
                nameTxt.Text = Path.GetFileName(path);
            } else { nameTxt.Text = ""; }

        }

        private void showLocation_TextChanged(object sender, TextChangedEventArgs e) {
        }

        private void nameTxt_TextChanged(object sender, TextChangedEventArgs e) {
            showNameTemp = nameTxt.Text;
            if (nameTxt.Text.Length >= 5 && nameTxt.Text != "Show name") {
                Action list;
                list = () => listShows();
                Thread thread = new Thread(list.Invoke);
                thread.Name = "List shows";
                thread.Start();
            }

        }
        struct Shows{
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
            option.confirm.Click += (s, e) => { selected(id); };
            panel.Children.Add(option);

        }
        private void showInfo(string specific) {
            MessageBox.Show(specific);
        }
        private void selected(string id) {
            MessageBox.Show(id+ "\n"+path);
        }
    }
}
