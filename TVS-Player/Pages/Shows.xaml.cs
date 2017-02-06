using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Shows.xaml
    /// </summary>
    public partial class Shows : Page {
        public Shows() {
            InitializeComponent();
            Action load;
            load = () => LoadShows();
            Thread t = new Thread(load.Invoke);
            t.Name = "Populate library";
            t.Start();
        }

        public void LoadShows() {
            for (int i = 0; i < DatabaseAPI.database.Shows.Count; i++) {
                Dispatcher.Invoke(new Action(() => {
                    GenerateRectangle(DatabaseAPI.database.Shows[i]);
                }), DispatcherPriority.Send);
            }
        }

        private void GenerateRectangle(SelectedShows ss) {
            ShowRectangle folder = new ShowRectangle(ss);
            folder.library = this;
            List.Children.Add(folder);
        }

        public void RemoveRectangle(ShowRectangle show) {
            List.Children.Remove(show);
        }
        private async void AddShowButton_MouseUp(object sender, MouseButtonEventArgs e) {
            Page showPage = new SelectShow();
            Window main = Window.GetWindow(this);
            ((MainWindow)main).AddTempFrame(showPage);
            var show = await Helpers.showSelector();
            string name = show.Item2;
            string id = show.Item1;
            DatabaseAPI.addShowToDb(id,name,true);
            Page selectLoc = new ScanLocation(true);
            ((MainWindow)main).AddTempFrame(selectLoc);
            Page refreshView = new Shows();
            ((MainWindow)main).SetFrameView(refreshView);

        }
    }
}
