using Newtonsoft.Json.Linq;

namespace ExtendQQ_DGJModule.Models
{
    public class ArtistInfo
    {
        public string Mid { get; }

        public string Name { get; }

        public ArtistInfo(string mid, string name)
        {
            Mid = mid;
            Name = name;
        }

        public static ArtistInfo Parse(JToken node)
        {
            return new ArtistInfo(node["mid"].ToObject<string>(), node["name"].ToString());
        }
    }
}
