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
using System.Windows.Threading;
using System.Globalization;
using TextBox = System.Windows.Controls.TextBox;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using System.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Page {
        public Startup() {
            InitializeComponent();
        }
       
        private void addDB_Click(object sender, RoutedEventArgs e) {
            addShow.Visibility = Visibility.Visible;

        }

        private void importDB_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Json files (*.json)|*.json";
            ofd.Multiselect = false;
            string path;
            string moveTo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TVS-Player\\db.json";
            var check = ofd.ShowDialog();
            if (check == DialogResult.OK) {
                path = ofd.FileName;
                if (File.Exists(moveTo)) {
                    File.Delete(moveTo);
                }
                File.Move(path,moveTo);
            }
        }
        string directory;
        string showNameTemp;
        string dbLoc;
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
                showLocation.Text = ofd.SelectedPath;
            }
            if (Path.GetFileName(directory) != "Downloads") {
                nameTxt.Text = Path.GetFileName(directory);
            } else { nameTxt.Text = ""; }

        }

        private void showLocation_TextChanged(object sender, TextChangedEventArgs e) {
            directory = showLocation.Text;
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
            MessageBox.Show(specific);
        }
        struct SelectedShows {
            private string idSel;
            private string pathSel;

            public SelectedShows(string id, string path) : this() {
                this.idSel = id;
                this.pathSel = path;
            }
        }
        List<SelectedShows> ShowList = new List<SelectedShows>();
        private void selected(string id, string showName) {
            if (Directory.Exists(directory)) {
                ShowList.Add(new SelectedShows(id, directory));
            } else {
                MessageBox.Show("Selected path doesn't exist", "Error");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            addShow.Visibility = Visibility.Hidden;
            selectNewLoc.Visibility = Visibility.Visible;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            List<SelectedShows> ShowList = new List<SelectedShows>();
            addShow.Visibility = Visibility.Hidden;
        }

        private void newDbLoc_TextChanged(object sender, TextChangedEventArgs e) {
            dbLoc = newDbLoc.Text;
        }

        private void newDbLoc_GotFocus(object sender, RoutedEventArgs e) {
            newDbLoc.Text = string.Empty;
            newDbLoc.GotFocus -= newDbLoc_GotFocus;
        }

        private void newDbLoc_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog();
            var check = fbd.ShowDialog();
            if (check == true) {
                dbLoc = fbd.SelectedPath;
            }
        }
    }
}
