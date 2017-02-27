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
using System.Collections.Generic;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for Shows.xaml
    /// </summary>
    public partial class Library : Page {
        public Library() {
            InitializeComponent();
            Action load;
            load = () => LoadShows();
            Thread t = new Thread(load.Invoke);
            t.Name = "Populate library";
            t.Start();
            Window m = Application.Current.MainWindow;
            ((MainWindow)m).SetTitle("Library");
        }

        public void LoadShows() {
            List<Show> list = DatabaseShows.ReadDb(); 
            foreach(Show s in list) { 
                Dispatcher.Invoke(new Action(() => {
                    GenerateRectangle(s);
                }), DispatcherPriority.Send);
            }
        }

        private void GenerateRectangle(Show ss) {
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
            DatabaseShows.AddShowToDb(Int32.Parse(id),name);
            Page selectLoc = new ScanLocation(true);
            ((MainWindow)main).AddTempFrame(selectLoc);
            Page refreshView = new Library();
            ((MainWindow)main).SetFrameView(refreshView);

        }
    }
}
