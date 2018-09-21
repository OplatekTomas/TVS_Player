using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player_Base {
    public class Episode {
        public int Id { get; set; }
        public int? AiredSeason { get; set; }
        public int? AiredEpisodeNumber { get; set; }
        public string EpisodeName { get; set; }
        public string FirstAired { get; set; }
        public List<string> GuestStars { get; set; } = new List<string>();
        public string Director { get; set; }
        public List<string> Directors { get; set; } = new List<string>();
        public List<string> Writers { get; set; } = new List<string>();
        public string Overview { get; set; }
        public string ShowUrl { get; set; }
        public int? AbsoluteNumber { get; set; }
        public int? SeriesId { get; set; }
        public int? AirsAfterSeason { get; set; }
        public int? AirsBeforeSeason { get; set; }
        public int? AirsBeforeEpisode { get; set; }
        public string ImdbId { get; set; }
        public double SiteRating { get; set; }
        public int? SiteRatingCount { get; set; }
        public string URL { get; set; }
        public bool FullInfo { get; set; }
        public bool Finished { get; set; }

        /// <summary>
        /// Returns epsisodes by seriesId. Server does not guarantee to return full data (check property FullInfo)
        /// </summary>
        /// <param name="seriesId"></param>
        /// <returns></returns>
        public static async Task<List<Episode>> GetEpisodes(int seriesId) {
            return (await Api.GetDataArray("api/GetEpisodes?seriesId=" + seriesId)).ToObject<List<Episode>>();
        }

        /// <summary>
        /// Returns episode by seriesId and episodeId. Guarantees that data will be Full
        /// </summary>
        /// <param name="seriesId"></param>
        /// <param name="episodeId"></param>
        /// <returns></returns>
        public static async Task<Episode> GetEpisode(int seriesId, int episodeId) {
            return (await Api.GetDataObject("api/GetEpisode?seriesId=" + seriesId + "&episodeId=" + episodeId)).ToObject<Episode>();
        }

    }
}
