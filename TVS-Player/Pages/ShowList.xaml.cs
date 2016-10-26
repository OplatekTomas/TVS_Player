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
        public ShowList() {
            InitializeComponent();
            frame.Content = new SearchShow();
        }
        string directory;



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

        }
        private void showLocation_TextChanged(object sender, TextChangedEventArgs e) {
            directory = showLocation.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
            Page showPage = new DbLocation();
            ((MainWindow)main).AddTempFrame(showPage);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            Window main = Window.GetWindow(this);
            ((MainWindow)main).CloseTempFrame();
        }
    }
}
