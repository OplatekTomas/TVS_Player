using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Threading;
using System.Globalization;
using TextBox = System.Windows.Controls.TextBox;
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
using System.Threading;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowList.xaml
    /// </summary>
    public partial class ShowList : Page {
        public ShowList(string nxt) {
            InitializeComponent();
            next = nxt;
            frame.Content = new SearchShow();
        }

        string next;

        private void Button_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            switch (next) {
                case "createdb":
                    List <Show> list = new List<Show>();
                    foreach(Show s in SearchShow.selectedShow) {
                        list.Add(s);
                    }
                    DatabaseShows.SaveDB(list);                 
                    ((MainWindow)main).CloseTempFrameIndex();
                    Page showPage = new DbLocation("nothing");
                    ((MainWindow)main).AddTempFrameIndex(showPage);
                    break;

                default:
                    ((MainWindow)main).CloseTempFrameIndex();
                    break;
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrameIndex();
        }
    }
}
