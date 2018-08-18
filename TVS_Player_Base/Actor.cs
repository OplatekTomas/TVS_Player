using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TVS_Player_Base {
    public class Actor {
        public int Id { get; set; }
        public int? SeriesId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int? SortOrder { get; set; }
        public string URL { get; set; }

        public static async Task<List<Actor>> GetActors(int seriesId) {
            return (await Api.GetDataArray("api/GetActors?seriesId=" + seriesId)).ToObject<List<Actor>>();
        }

        public static async Task<Actor> GetActor(int actorId) {
            return (await Api.GetDataObject("api/GetActor?actorId=" + actorId)).ToObject<Actor>();
        }
    }
}
