using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace TVSPlayer {
    class SrtParser {
        // Properties -----------------------------------------------------------------------

        private readonly string[] _delimiters = { "-->", "- >", "->" };


        // Constructors --------------------------------------------------------------------

        public SrtParser() { }


        // Methods -------------------------------------------------------------------------

        public List<Subtitles> ParseStream(Stream srtStream, Encoding encoding) {
            srtStream.Position = 0;
            var reader = new StreamReader(srtStream, encoding, true);
            var items = new List<Subtitles>();
            var srtSubParts = GetSrtSubTitleParts(reader).ToList();
            foreach (var srtSubPart in srtSubParts) {
                var lines = srtSubPart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(s => s.Trim()).Where(l => !String.IsNullOrEmpty(l)).ToList();
                var item = new Subtitles();
                foreach (var line in lines) {
                    if (item.StartTime == 0 && item.EndTime == 0) {
                        if (TryParseTimecodeLine(line, out double startTc, out double endTc)) {
                            item.StartTime = startTc;
                            item.EndTime = endTc;
                        }
                    } else {
                        item.Lines.Add(GetPanel(line));
                    }
                }
                if ((item.StartTime != 0 || item.EndTime != 0) && item.Lines.Any()) {
                    // parsing succeeded
                    items.Add(item);
                }
            }
            return items;
        }

        private StackPanel GetPanel(string line) {
            StackPanel panel = new StackPanel();
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Orientation = Orientation.Horizontal;
            panel.Margin = new Thickness(0, 0, 0, 10);
            var block = GetTextBlock();
            Regex rgx = new Regex("<.*>.*</.*>", RegexOptions.IgnoreCase);
            var matches = rgx.Matches(line);
            if (matches.Count > 0) {
                foreach (var match in matches) {

                }
            } else {
                block.Text = line;
                panel.Children.Add(block);
            }
            return panel;
        }

        private TextBlock EditText(TextBlock block, string text) {

        }

        private TextBlock GetTextBlock() {
            DropShadowEffect effect = new DropShadowEffect();
            effect.Color = Colors.Black;
            effect.BlurRadius = 1;
            effect.ShadowDepth = 3;
            TextBlock block = new TextBlock();
            block.Effect = effect;
            block.HorizontalAlignment = HorizontalAlignment.Center;
            block.FontSize = 36;
            block.Foreground = GetColorFromHex("#F5F5F5");
            return block;
        }

        private Brush GetColorFromHex(string hex) {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        /// <summary>
        /// Splits srt file to lines
        /// </summary>
        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader) {
            string line;
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null) {
                if (string.IsNullOrEmpty(line.Trim())) {
                    var res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res)) {
                        yield return res;
                    }
                    sb = new StringBuilder();
                } else {
                    sb.AppendLine(line);
                }
            }
            if (sb.Length > 0) {
                yield return sb.ToString();
            }
        }

        /// <summary>
        /// Tries to parse line to start and end in double
        /// </summary>
        private bool TryParseTimecodeLine(string line, out double startTc, out double endTc) {
            var parts = line.Split(_delimiters, StringSplitOptions.None);
            if (parts.Length != 2) {
                // this is not a timecode line
                startTc = endTc = -1;
                return false;
            } else {
                startTc = ParseSrtTimecode(parts[0]);
                endTc = ParseSrtTimecode(parts[1]);
                return true;
            }
        }

        /// <summary>
        /// Parse string to double
        /// </summary>
        private static double ParseSrtTimecode(string s) {
            var match = Regex.Match(s, "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
            if (match.Success) {
                s = match.Value;
                TimeSpan result;
                if (TimeSpan.TryParse(s.Replace(',', '.'), out result)) {
                    var nbOfMs = result.TotalMilliseconds;
                    return nbOfMs;
                }
            }
            return -1;
        }
    }
}
