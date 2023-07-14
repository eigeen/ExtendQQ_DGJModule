using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace ExtendQQ_DGJModule.Models
{
    public class SongInfo
    {
        public string Mid { get; }

        public string Name { get; }

        public ArtistInfo[] Artists { get; }

        public string? Url { get; set; }


        public SongInfo(string mid, string name, ArtistInfo[] artists)
        {
            Mid = mid;
            Name = name;
            Artists = artists;
        }

        public static SongInfo Parse(JToken node)
        {
            return new SongInfo(node["mid"].ToString(),
                node["name"].ToString(),
                node["singer"].Select(ArtistInfo.Parse).ToArray());
        }
    }
}