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

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Shows.xaml
    /// </summary>
    public partial class Shows : Page {
        public Shows(){
            InitializeComponent();
            Random r = new Random();
            for (byte i = 0; i < 10; i++){
                Grid folder = new Grid();
                GenerateRectangle(out folder, r);
            }
        }

        private void GenerateRectangle(out Grid folder, Random r){
            var xaml = XamlWriter.Save(BaseRectangle);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            folder = (Grid)XamlReader.Load(xmlReader);
            Rectangle rect = (Rectangle)folder.Children[0];
            Color genCol = Color.FromRgb((byte)(r.NextDouble() * 255), (byte)(r.NextDouble() * 255), (byte)(r.NextDouble() * 255));
            rect.Fill = new SolidColorBrush(genCol);
            rect.MouseLeftButtonDown += ShowClicked_Event;
            if (genCol.R > 160 || genCol.G > 160 || genCol.B > 160){
                ((TextBlock)folder.Children[1]).Foreground = new SolidColorBrush(Colors.Black);
            }
            List.Children.Add(folder);
        }
        private void Quit_Event(object sender, RoutedEventArgs e){
            Application.Current.Shutdown();
        }

        private void ShowClicked_Event(object sender, MouseButtonEventArgs e) {
            Page showPage = new ShowInfo();
            ((ShowInfo)showPage).lastPage = this;
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }
    }
}
