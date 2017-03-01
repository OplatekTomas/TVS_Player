using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TVS_Player {
    class FindTorrent {
        public static List<TorrentItem> GetTorrents(string show, int season, int episode) {
            string url = GetUrl(show, season, episode);
            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = htmlWeb.Load(url);
            List<HtmlNode> rows = new List<HtmlNode>();
            try {
                rows = htmlDocument.DocumentNode.SelectSingleNode("//table").ChildNodes[3].SelectNodes("//tr").ToList();
            } catch (Exception) {
                return new List<TorrentItem>();
            }
            rows.RemoveAt(0);
            List<TorrentItem> tList = new List<TorrentItem>();
            foreach (HtmlNode row in rows) {
                TorrentItem t = new TorrentItem();
                t.quality = "std";
                t.name = row.ChildNodes[1].InnerText;
                if (t.name.Contains("720p")) {
                    t.quality = "720p";
                }
                if (t.name.Contains("1080p")) {
                    t.quality = "1080p";
                }
                if (t.name.Contains("2160p")) {
                    t.quality = "2160p";
                }
                url = "http://1337x.to" + row.ChildNodes[1].ChildNodes[1].Attributes[0].Value;
                htmlDocument = htmlWeb.Load(url);
                List<HtmlNode> a = htmlDocument.DocumentNode.SelectNodes("//ul").ToList();
                t.magnet = a[5].ChildNodes[7].ChildNodes[0].Attributes[1].Value;
                t.seeders = Int32.Parse(row.ChildNodes[3].InnerText);
                t.size = row.ChildNodes[9].ChildNodes[0].InnerText;
                tList.Add(t);
            }
            return tList;
        }
        public static List<TorrentItem> GetTorrents(string show, int season, int episode,string quality) {
            List<TorrentItem> orig = GetTorrents(show, season, episode);
            List<TorrentItem> newList = new List<TorrentItem>();
            foreach (TorrentItem t in orig) {
                if (t.quality == quality) {
                    newList.Add(t);
                }
            }
            return newList;
        }
        public static TorrentItem GetBestTorrent(string show, int season, int episode, string quality) {
            List<TorrentItem> orig = GetTorrents(show, season, episode);
            List<TorrentItem> newList = new List<TorrentItem>();
            foreach (TorrentItem t in orig) {
                if (t.quality == quality) {
                    newList.Add(t);
                }
            }
            TorrentItem torrent = newList[0];
            for (int i = 0; i < newList.Count; i++) {
                if (newList[i].seeders > torrent.seeders) {
                    torrent = newList[i];
                }
            }        
            return torrent;
        }
        public static TorrentItem GetBestTorrent(string show, int season, int episode) {
            List<TorrentItem> orig = GetTorrents(show, season, episode);
            TorrentItem torrent = orig[0];
            for (int i = 0; i < orig.Count; i++) {
                if (orig[i].seeders > torrent.seeders) {
                    torrent = orig[i];
                }
            }
            return torrent;
        }

        private static string GetUrl(string show, int season, int episode) {
            string url = removeYear(show).Replace(" ", "+");
            if (season < 10) {
                if (episode < 10) {
                    url += "+S0"+season+"E0"+episode;
                } else if (episode >= 10) {
                    url += "+S0" + season + "E" + episode;
                }
            } else if (season >= 10) {
                if (episode < 10) {
                    url += "+S" + season + "E0" + episode;
                } else if (episode >= 10) {
                    url += "+S" + season + "E" + episode;
                }

            }
            return "http://1337x.to/search/"+ url+"/1/";
        }
        private static string removeYear(string name) {
            Regex reg = new Regex(@"\([0-9]{4}\)");
            Match regMatch = reg.Match(name);
            if (regMatch.Success) {
                return reg.Replace(name, "");
            } else { return name; }
        }

    }   
    public class TorrentItem {
        public string magnet { get; set; }
        public string name { get; set; }
        public string quality { get; set; }
        public int seeders { get; set; }
        public string size { get; set; }
    }
}
