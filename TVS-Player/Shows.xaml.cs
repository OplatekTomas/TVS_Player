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
                ShowRectangle folder = new ShowRectangle();
                GenerateRectangle(out folder, r);
            }
        }

        private void GenerateRectangle(out ShowRectangle folder, Random r){
            folder = new ShowRectangle();
            List.Children.Add(folder);
        }
    }
}
