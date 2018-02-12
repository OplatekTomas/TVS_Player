using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TVS.API;

namespace TVSPlayer {
    public enum TorrentQuality { Standart, HD, FHD, UHD }

    public class Torrent {

        public string Magnet { get; set; }
        public string Name { get; set; }
        public TorrentQuality Quality { get; set; }
        public int Seeders { get; set; }
        public int Leech { get; set; }
        public string Size { get; set; }
        public Series Series { get; set; }
        public int seriesId;
        public int episodeId;
        public string URL { get; set; }
        public Episode Episode { get; set; }
        public bool HasFinished { get; set; } = false;
        public bool IsSequential { get; set; } = false;
        public string FinishedAt { get; set; } = "-";

        /// <summary>
        /// Searches for torrents with any quality. Use with causion - you might get banned
        /// </summary>
        /// <param name="series">Series to search for</param>
        /// <param name="episode">Episode to serach for</param>
        /// <returns></returns>
        public async static Task<List<Torrent>> Search(Series series, Episode episode) {
            return await Task.Run(() => {
                string url = GetUrl(series.seriesName, episode.airedSeason, episode.airedEpisodeNumber);
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument htmlDocument = htmlWeb.Load(url);
                List<HtmlNode> rows = new List<HtmlNode>();
                try {
                    rows = htmlDocument.DocumentNode.SelectSingleNode("//table").ChildNodes[3].SelectNodes("//tr").ToList();
                } catch (Exception) {
                    return null;
                }
                rows.RemoveAt(0);
                List<Torrent> tList = new List<Torrent>();
                foreach (HtmlNode row in rows) {
                    Torrent t = new Torrent();
                    t.Quality = TorrentQuality.Standart;
                    t.Name = row.ChildNodes[1].ChildNodes[1].InnerHtml;
                    if (t.Name.Contains("720p")) {
                        t.Quality = TorrentQuality.HD;
                    }
                    if (t.Name.Contains("1080p")) {
                        t.Quality = TorrentQuality.FHD;
                    }
                    if (t.Name.Contains("2160p")) {
                        t.Quality = TorrentQuality.UHD;
                    }
                    t.URL = "http://1337x.to" + row.ChildNodes[1].ChildNodes[1].Attributes[0].Value;
                    t.Seeders = Int32.Parse(row.ChildNodes[3].InnerText);
                    t.Leech = Int32.Parse(row.ChildNodes[5].InnerText);
                    t.Size = row.ChildNodes[9].ChildNodes[0].InnerText;
                    t.Series = series;
                    t.Episode = episode;
                    tList.Add(t);
                }
                return tList;
            });
        }

        /// <summary>
        /// Searches 1337x.to for torrents with specified quality. Use with causion - you might get banned
        /// </summary>
        /// <param name="series">Series to search for</param>
        /// <param name="episode">Episode to search for</param>
        /// <param name="quality">Quality to search for</param>
        /// <returns></returns>
        public async static Task<List<Torrent>> Search(Series series, Episode episode, TorrentQuality quality) {
            return (await Search(series, episode))?.Where(x => x.Quality == quality).ToList();
        }

        /// <summary>
        /// Searches 1337x.to for a single torrent with any quality. Use with causion - you might get banned
        /// </summary>
        /// <param name="series">Series to search for</param>
        /// <param name="episode">Episode to search for</param>
        /// <returns>The one with most seeders</returns>
        public async static Task<Torrent> SearchSingle(Series series, Episode episode) {
            return (await Search(series, episode))?.OrderByDescending(x => x.Seeders).FirstOrDefault();
        }

        /// <summary>
        /// Searches 1337x.to for a single torrent with specified quality. Use with causion - you might get banned
        /// </summary>
        /// <param name="series">Series to search for</param>
        /// <param name="episode">Episode to search for</param>
        /// <param name="quality">Quality to search for</param>
        /// <returns>The one with most seeders</returns>
        public async static Task<Torrent> SearchSingle(Series series, Episode episode, TorrentQuality quality) {
            var tor = await Search(series, episode);
            if (tor != null) {
                return tor.Where(x => x.Quality == quality).OrderByDescending(x => x.Seeders).FirstOrDefault();
            }
            return null;
        }

        private static string GetUrl(string show, int? season, int? episode) {
            string url = RemoveYear(show).Replace(" ", "+");
            if (season < 10) {
                url += episode < 10 ? "+S0" + season + "E0" + episode : "+S0" + season + "E" + episode;
            } else {
                url += episode < 10 ? "+S" + season + "E0" + episode : "+S" + season + "E" + episode;             
            }
            return "http://1337x.to/search/" + url + "/1/";
        }

        private static string RemoveYear(string text) {
            Regex reg = new Regex(@"\([0-9]{4}\)");
            Match regMatch = reg.Match(text);
            return regMatch.Success ? reg.Replace(text, "") : text;
        }           
      

    }
   
}