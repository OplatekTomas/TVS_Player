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

namespace TVS_Player {
    /// <summary>
    /// Interaction logic for PosterSelector.xaml
    /// </summary>
    public partial class PosterSelector : UserControl {
        bool isSelected = false;
        int subNumber;
        SelectShowPoster creator;
        public string path;
        public PosterSelector() {
            InitializeComponent();
        }
        public PosterSelector(String path,int i,SelectShowPoster parent) {
            InitializeComponent();
            Thumbnail.Source = new BitmapImage(new Uri(path));
            this.path = path;
            subNumber = i;
            creator = parent;
        }

        private void ImageClicked_Event(object sender, MouseButtonEventArgs e) {
            if (isSelected) {
                border.BorderThickness = new Thickness(0);
                creator.selected = null;
                isSelected = false;
            } else {
                border.BorderThickness = new Thickness(3);
                foreach (PosterSelector ps in ((WrapPanel)this.Parent).Children) {
                    if (ps != this) {
                        ps.ResetSelection();
                    }
                }
                creator.selected = this;
                isSelected = true;
            }
        }
        public void ResetSelection() {
            isSelected = false;
            border.BorderThickness = new Thickness(0);
        }
    }
}
