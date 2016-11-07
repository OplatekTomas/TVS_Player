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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowInfo.xaml
    /// </summary>
    public partial class ShowInfo : Page {
        ShowRectangle sr;
        public ShowInfo() {
            InitializeComponent();
        }

        public ShowInfo(int ID, ShowRectangle sr) {
            this.sr = sr;
            InitializeComponent();
            Action getInfo;
            getInfo = () => GetInfo(sr.ID);
            Thread t = new Thread(getInfo.Invoke);
            t.Start();
            Action getEps;
            getEps = () => GetEpInfo(sr.ID);
            Thread tep = new Thread(getEps.Invoke);
            tep.Start();

        }
        public void GetInfo(int ID) {
            Dispatcher.Invoke(new Action(() => {
                JmenoSerialu.Content = sr.ShowName;
            }), DispatcherPriority.Send);
            JObject jo = JObject.Parse(Api.apiGet(ID));
            Dispatcher.Invoke(new Action(() => {
                Popisek.Text = jo["data"]["overview"].ToString();
                Rok.Text = jo["data"]["firstAired"].ToString();
            }), DispatcherPriority.Send);
            Api.apiGetPoster(ID, false);
            Dispatcher.Invoke(new Action(() => {
                Obrazek.Source = new BitmapImage(new Uri(Helpers.path + "//" + sr.ID + "//" + sr.filename));
            }), DispatcherPriority.Send);
        }
        private void ReturnBack_Event(object sender, MouseButtonEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView((Page)new Shows());
        }
        public void GetEpInfo(int ID) {
            JObject series = JObject.Parse(Api.apiGetSeasons(ID));
            int count = series["data"]["airedSeasons"].Count();
            foreach (JToken jt in series["data"]["airedSeasons"]) {
                if (jt.Value<int>() == 0) {
                    count--;
                }
            }
            for (int index = 1; index <= count; index++) {
                foreach (JToken jt in series["data"]["airedSeasons"]) {
                    if (index == jt.Value<int>()) {
                        int i = Int32.Parse(jt.ToString());
                        JObject eps = JObject.Parse(Api.apiGetEpisodesBySeasons(ID, i));
                        foreach (JToken ep in eps["data"]) {
                            Dispatcher.Invoke(new Action(() => {
                                TextBlock tb = new TextBlock();
                                if (ep["overview"].Value<String>() != null) {
                                    tb.Text = ep["episodeName"].ToString() + " - " + ep["overview"].Value<String>().Replace("\r\n", "");
                                } else {
                                    tb.Text = ep["episodeName"].ToString();
                                }
                                tb.Foreground = new SolidColorBrush(Colors.White);
                                tb.Margin = new Thickness(5);
                                EpisodesList.Children.Add(tb);
                            }), DispatcherPriority.Send);
                        }
                    }
                }
            }
        }
    }
}
