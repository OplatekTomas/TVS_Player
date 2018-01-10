using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TVSPlayer {
    class Subtitles {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public List<StackPanel> Lines { get; set; } = new List<StackPanel>();

        public static List<Subtitles> ParseSubtitles(string file) {
            List<Subtitles> subs = new List<Subtitles>();
            if (File.Exists(file)) {
                switch (Path.GetExtension(file)) {
                    case ".srt":
                        subs.AddRange(new SrtParser().ParseStream(file, GetEncoding(file)));
                        break;
                }
            }
            return subs;
        }

        public static Encoding GetEncoding(string filename) {
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
                file.Read(bom, 0, 4);
            }
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }
    }
}
