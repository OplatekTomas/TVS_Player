using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TVSPlayer {
    class Subtitles {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public List<StackPanel> Lines { get; set; } = new List<StackPanel>();

        public List<Subtitles> ParseSubtitles(string item) {
            return new List<Subtitles>();
        }
    }
}
