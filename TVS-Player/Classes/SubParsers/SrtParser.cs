using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace TVS_Player {
    class SrtParser {
        private static readonly string[] _delimiters = { "-->", "- >", "->" };


        // Constructors --------------------------------------------------------------------

        public SrtParser() { }


        // Methods -------------------------------------------------------------------------

        public static List<SubtitleItem> ParseStream(Encoding encoding) {
            Stream srtStream = File.Open(@"E:\01Lib\Game of Thrones\Season 06\Game of Thrones - S06E01 - The Red Woman.srt", FileMode.Open);
            // test if stream if readable and seekable (just a check, should be good)
            if (!srtStream.CanRead || !srtStream.CanSeek) {
                var message = string.Format("Stream must be seekable and readable in a subtitles parser. " +
                                   "Operation interrupted; isSeekable: {0} - isReadable: {1}",
                                   srtStream.CanSeek, srtStream.CanSeek);
                throw new ArgumentException(message);
            }

            // seek the beginning of the stream
            srtStream.Position = 0;

            var reader = new StreamReader(srtStream, encoding, true);

            var items = new List<SubtitleItem>();
            var srtSubParts = GetSrtSubTitleParts(reader).ToList();
            if (srtSubParts.Any()) {
                foreach (var srtSubPart in srtSubParts) {
                    var lines =
                        srtSubPart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                            .Select(s => s.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

                    var item = new SubtitleItem();
                    foreach (var line in lines) {
                        if (item.StartTime == 0 && item.EndTime == 0) {
                            // we look for the timecodes first
                            int startTc;
                            int endTc;
                            var success = TryParseTimecodeLine(line, out startTc, out endTc);
                            if (success) {
                                item.StartTime = startTc;
                                item.EndTime = endTc;
                            }
                        } else {
                            Tuple<char, string> t = GetStyle(line);
                            item.TextStyle = t.Item1;
                            // we found the timecode, now we get the text
                            item.Lines.Add(t.Item2);
                        }
                    }

                    if ((item.StartTime != 0 || item.EndTime != 0) && item.Lines.Any()) {
                        // parsing succeeded
                        items.Add(item);
                    }
                }

                if (items.Any()) {
                    return items;
                } else {
                    throw new ArgumentException("Stream is not in a valid Srt format");
                }
            } else {
                throw new FormatException("Parsing as srt returned no srt part.");
            }
        }

        private static Tuple<char, string> GetStyle(string line) {
            string[] separator = { "</i>", "<i>", "</?b>", "<font ?color=#[0-9a-zA-Z]{3,8}>","</font>" };
            Tuple<char, string> t = new Tuple<char, string>('n', line);
            string text = line;
            for (int i = 0; i < separator.Length; i++) {
                Regex reg = new Regex(separator[i]);
                Match m = reg.Match(text);
                while (m.Success) {
                    m = reg.Match(text);
                    if (m.Success) {
                        text = text.Remove(m.Index, m.Length);
                        switch (i) {
                            case 0: case 1:
                                t = new Tuple<char, string>('i', text);
                                break;
                            case 3: case 4:
                                t = new Tuple<char, string>('n', text);
                                break;
                        }
                    }
                }
            }
            return t;
        }


        /// <summary>
        /// Enumerates the subtitle parts in a srt file based on the standard line break observed between them. 
        /// A srt subtitle part is in the form:
        /// 
        /// 1
        /// 00:00:20,000 --> 00:00:24,400
        /// Altocumulus clouds occur between six thousand
        /// 
        /// </summary>
        /// <param name="reader">The textreader associated with the srt file</param>
        /// <returns>An IEnumerable(string) object containing all the subtitle parts</returns>
        private static IEnumerable<string> GetSrtSubTitleParts(TextReader reader) {
            string line;
            var sb = new StringBuilder();

            while ((line = reader.ReadLine()) != null) {
                if (string.IsNullOrEmpty(line.Trim())) {
                    // return only if not empty
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

        private static bool TryParseTimecodeLine(string line, out int startTc, out int endTc) {
            var parts = line.Split(_delimiters, StringSplitOptions.None);
            if (parts.Length != 2) {
                // this is not a timecode line
                startTc = -1;
                endTc = -1;
                return false;
            } else {
                startTc = ParseSrtTimecode(parts[0]);
                endTc = ParseSrtTimecode(parts[1]);
                return true;
            }
        }

        /// <summary>
        /// Takes an SRT timecode as a string and parses it into a double (in seconds). A SRT timecode reads as follows: 
        /// 00:00:20,000
        /// </summary>
        /// <param name="s">The timecode to parse</param>
        /// <returns>The parsed timecode as a TimeSpan instance. If the parsing was unsuccessful, -1 is returned (subtitles should never show)</returns>
        private static int ParseSrtTimecode(string s) {
            var match = Regex.Match(s, "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
            if (match.Success) {
                s = match.Value;
                TimeSpan result;
                if (TimeSpan.TryParse(s.Replace(',', '.'), out result)) {
                    var nbOfMs = (int)result.TotalMilliseconds;
                    return nbOfMs;
                }
            }
            return -1;
        }
    }
}
