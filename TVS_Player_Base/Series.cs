using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TVS_Player_Base
{
    public class Series{
        public int Id { get; set; }
        public string SeriesName { get; set; }
        public List<string> aliases = new List<string>();
        public string Status { get; set; }
        public string FirstAired { get; set; }
        public string Network { get; set; }
        public string Runtime { get; set; }
        public List<string> Genre { get; set; } = new List<string>();
        public string Overview { get; set; }
        public string AirsDayOfWeek { get; set; }
        public string AirsTime { get; set; }
        public string Rating { get; set; }
        public string ImdbId { get; set; }
        public double SiteRating { get; set; }
        public int? SiteRatingCount { get; set; }
        public string URL { get; set; }

        public static async Task<List<Series>> GetSeries() {
            return (await Api.GetDataArray("api/GetSeries")).ToObject<List<Series>>();
        }

        public static async Task<Series> GetSeries(int seriesId) {
            return (await Api.GetDataObject("api/GetSeries?seriesId=" + seriesId)).ToObject<Series>();
        }

        public static async Task<List<Series>> SearchSeries(string query) {
            return (await Api.GetDataArray("api/SearchSeries?query=" + query)).ToObject<List<Series>>();
        }
    }
}
