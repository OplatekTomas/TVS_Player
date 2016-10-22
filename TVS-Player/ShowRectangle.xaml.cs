using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for ShowRectangle.xaml
    /// </summary>
    public partial class ShowRectangle : Grid {
        public int ID = 121361;
        public ShowRectangle() {
            InitializeComponent();
            Api.apiGetPoster(ID);
            String path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\TVS-Player\\" + ID.ToString() + "\\" + ID.ToString() + ".jpg";
            Image.Source = new BitmapImage(new Uri(path));
        }

        private void ShowClicked_Event(object sender, MouseButtonEventArgs e) {
            Page showPage = new ShowInfo(ID);
            Window main = Window.GetWindow(this);
            ((MainWindow)main).SetFrameView(showPage);
        }
    }
}
