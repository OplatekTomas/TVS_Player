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
        private void listShows() {
            List<Show> s = Api.apiGet(showNameTemp);
            Dispatcher.Invoke(new Action(() => {
                panel.Children.Clear();
                foreach(Show show in s) { 
                    tvShowControl option = new tvShowControl();
                    option.showName.Text = show.name;
                    option.firstAir.Text = show.release;
                    option.info.Click += (se, e) => { showInfo(show.id.TVDb); };
                    option.confirm.Click += (se, e) => { selected(show); };
                    panel.Children.Add(option);
                    }
                }), DispatcherPriority.Send);
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
