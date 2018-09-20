using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player_Base {
    public class ScannedFile {
        public int Id { get; set; }
        public string URL { get; set; }
        public string Extension { get; set; }
        public string FileType { get; set; }
        public string SubtitleLanguage { get; set; }
        public string TimeStamp { get; set; }
        public int Resolution { get; set; } = 0;
        public int SeriesId { get; set; }
        public int EpisodeId { get; set; }

        public static async Task<List<ScannedFile>> GetFiles(int episodeId) {
            return (await Api.GetDataArray("api/GetFiles?episodeId=" + episodeId)).ToObject<List<ScannedFile>>();
        }

        public static async Task<ScannedFile> GetFile(int episodeId, int fileId) {
            return (await Api.GetDataObject("api/GetActor?episodeId=" + episodeId + "&fileId=" + fileId)).ToObject<ScannedFile>();
        }

        /// <summary>
        /// Sends progress that has been viewed on some episode
        /// </summary>
        /// <param name="progress">Progress viewed in seconds</param>
        /// <returns></returns>
        public static async Task SetEpisodeProgress(int episodeId, int fileId, double progress) {
            await Api.PostData("api/SetEpisodeProgress", "{ \"episodeId\":" + episodeId + ", \"fileId\":" + fileId + ", \"progress\": " + progress + "}");
        }

        public static async Task SetEpisodeFinished(int episodeId, bool isFinished) {
            await Api.PostData("api/SetEpisodeFinished", "{ \"episodeId\":" + episodeId + ", \"isFinished\":" + (isFinished ? 1 : 0) + "}");
        }
    }
}
