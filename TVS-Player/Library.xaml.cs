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
using System.Windows.Shapes;

namespace TVS_Player
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : Window
    {
        public Library()
        {
            InitializeComponent();
            Random r = new Random();
            for (byte i = 0; i < 10; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Width = 180;
                rect.Height = 200;
                rect.Fill = new SolidColorBrush(Color.FromRgb((byte)(r.NextDouble() * 255), (byte)(r.NextDouble() * 255), (byte)(r.NextDouble() * 255)));
                rect.Margin = new Thickness(10);
                List.Children.Add(rect);
            }

        }
    }
}
