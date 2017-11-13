using System;
using System.Collections.Generic;
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

namespace TVSPlayer
{

    public enum MessageBoxButtons { YesNoCancel }
    public enum MessageBoxResult { Yes, No, Cancel };

    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Page
    {
        private MessageBox(string text, string header, MessageBoxButtons? buttons){
            InitializeComponent();
            TextContent = text;
            HeaderText = header;
            Buttons = buttons;
        }

        string HeaderText { get; set; }
        string TextContent { get; set; }
        MessageBoxButtons? Buttons { get; set; }

        static MessageBoxResult? result = null;

        public async static Task<MessageBoxResult> Show(string text) {
            return await Show(text, null, null);
        }
        public async static Task<MessageBoxResult> Show(string text, string header) {
            return await Show(text, header, null);
        }

        public async static Task<MessageBoxResult> Show(string text, string header, MessageBoxButtons? buttons) {
            MainWindow.AddPage(new MessageBox(text, header, buttons));
            return await Task.Run(() => {
                while (result == null) {
                    Thread.Sleep(50);
                }
                Application.Current.Dispatcher.Invoke(() => { MainWindow.RemovePage(); }, System.Windows.Threading.DispatcherPriority.Send);
                var temp = result;
                result = null;
                return (MessageBoxResult)temp;
            });
        }

        private void Close_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            result = MessageBoxResult.Cancel;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Content.Text = TextContent;
            Header.Text = String.IsNullOrEmpty(HeaderText) ? "" : HeaderText;
            if (Buttons == MessageBoxButtons.YesNoCancel) {
                YesNo.Visibility = Visibility.Visible;
                NoButtons.Visibility = Visibility.Collapsed;
            } else {
                YesNo.Visibility = Visibility.Collapsed;
                NoButtons.Visibility = Visibility.Visible;
            }
        }

        private void Yes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            result = MessageBoxResult.Yes;
        }

        private void No_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            result = MessageBoxResult.No;
        }

        private void Close_MouseEnter(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e) {
            Mouse.OverrideCursor = null;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
           // MiddleColumn.Width = LeftColumn/30
        }
    }
}
