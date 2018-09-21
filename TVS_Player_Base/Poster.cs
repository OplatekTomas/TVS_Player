using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player_Base {
    public class Poster {
        public int Id { get; set; }
        public string KeyType { get; set; }
        public string SubKey { get; set; }
        public string FileName { get; set; }
        public string Resolution { get; set; }
        public Ratings RatingsInfo { get; set; }
        public string Thumbnail { get; set; }
        public class Ratings {
            public double Average { get; set; }
            public int Count { get; set; }
        }
        public string URL { get; set; }

        /// <summary>
        /// Returns info about all posters for Series
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Poster>> GetPosters(int seriesId) {
            return (await Api.GetDataArray("api/GetPosters?seriesId=" + seriesId)).ToObject<List<Poster>>();
        }
        /// <summary>
        /// Returns info for single poster by posterId
        /// </summary>
        /// <returns></returns>
        public static async Task<Poster> GetPoster(int posterId) {
            return (await Api.GetDataObject("api/GetPoster?posterId=" + posterId)).ToObject<Poster>();
        }

        /// <summary>
        /// Gets background image (FanArt) for series
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public static async Task<Poster> GetBackground(int seriesId) {
            return (await Api.GetDataObject("api/GetBackground?seriesId=" + seriesId)).ToObject<Poster>();
        }

    }
}
