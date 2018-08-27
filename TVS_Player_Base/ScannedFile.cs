using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player_Base {
    public class ScannedFile {
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
    }
}
