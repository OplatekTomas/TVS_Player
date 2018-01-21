using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TVS.API;

namespace TVSPlayer {

    public class SubtitleItem {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public List<StackPanel> Lines { get; set; } = new List<StackPanel>();
    }
    public enum Language { English, Czech, Other };
    public class Subtitles {
        public Language Language { get; set; }
        public string FileName { get; set; }
        public Episode Episode { get; set; }
        public HttpWebRequest DownloadLink { get; set; }
        public string Version { get; set; }

        public static List<SubtitleItem> ParseSubtitleItems(string file) {
            List<SubtitleItem> subs = new List<SubtitleItem>();
            if (File.Exists(file)) {
                switch (Path.GetExtension(file)) {
                    case ".srt":
                        subs.AddRange(new SrtParser().ParseStream(file, GetEncoding(file)));
                        break;
                }
            }
            return subs;
        }

        public static List<Subtitles> GetSubtitles(Episode.ScannedFile file) {
            List<Subtitles> subList = new List<Subtitles>();
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load("http://www.addic7ed.com/search.php?search=" + Path.GetFileNameWithoutExtension(file.OriginalName) + "&anti_cache=" + DateTime.Now.ToString());
            var items = htmlDocument.DocumentNode.SelectNodes("//div[@id='container95m']");
            items.RemoveAt(items.Count-1);
            foreach (var item in items) {
                Subtitles subs = new Subtitles();
                var data = item.ChildNodes[1].ChildNodes[3].ChildNodes[3].ChildNodes[1];
                subs.Version = data.ChildNodes[1].ChildNodes[1].InnerText.Remove(data.ChildNodes[1].ChildNodes[1].InnerText.IndexOf(',')).Remove(0, 8);
                string url = "http://www.addic7ed.com";
                if (data.ChildNodes[4].ChildNodes[8].ChildNodes[2].Name == "a") {
                    url += data.ChildNodes[4].ChildNodes[8].ChildNodes[2].Attributes[1].Value;
                } else {
                    url+= data.ChildNodes[4].ChildNodes[8].ChildNodes[5].Attributes[1].Value;
                }
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Referer = htmlWeb.ResponseUri.ToString();
                subs.DownloadLink = request;
                subList.Add(subs);
            }

            return subList;
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
