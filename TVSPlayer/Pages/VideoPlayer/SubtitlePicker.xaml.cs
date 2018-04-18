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

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for SubtitlePicker.xaml
    /// </summary>
    public partial class SubtitlePicker : UserControl {
        public SubtitlePicker() {
            InitializeComponent();
        }
        public SubtitlePicker(LocalPlayer.FoundSubtitle subtitle) {
            InitializeComponent();
            Subtitle = subtitle;
        }
        public LocalPlayer.FoundSubtitle Subtitle { get; set; }
    }
}
