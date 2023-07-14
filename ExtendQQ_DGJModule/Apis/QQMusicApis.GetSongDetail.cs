using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtendQQ_DGJModule.Exceptions;
using ExtendQQ_DGJModule.Extensions;
using ExtendQQ_DGJModule.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExtendQQ_DGJModule.Apis
{
    public static partial class QQMusicApis
    {
        private static Task<JToken> GetSongDetailCore(HttpClient client, string songId,
            CancellationToken token = default)
        {
            // {
            //     "req_0": {
            //         "module": "music.pf_song_detail_svr",
            //         "method": "get_song_detail_yqq",
            //         "param": {
            //             "song_mid": "0039MnYb0qxYhV"
            //         }
            //     }
            // }
            var data = new Dictionary<string, object>
            {
                ["req_0"] = new Dictionary<string, object>
                {
                    ["method"] = "get_song_detail_yqq",
                    ["module"] = "music.pf_song_detail_svr",
                    ["param"] = new Dictionary<string, object>
                    {
                        ["song_mid"] = songId,
                    }
                }
            };

            return client.PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", JsonConvert.SerializeObject(data), token)
                .GetJsonAsync(token);
        }

        public static async Task<SongInfo> GetSongDetailAsync(HttpClient client, string songId,
            CancellationToken token = default)
        {
            var root = (JObject)await GetSongDetailCore(client, songId, token).ConfigureAwait(false);
            if (root["req_0"]["code"].ToObject<int>() != 0)
            {
                throw new UnknownResponseException(root, $"获取歌曲信息出错 原始内容{root}");
            }

            try
            {
                var info = root["req_0"]["data"]["track_info"];
                var singers = info["singer"];
                var artists = singers.Select(
                    s => new ArtistInfo(s["mid"].ToString(), s["name"].ToString())).ToArray();

                var songInfo = new SongInfo(songId, info["name"].ToString(), artists);
                return songInfo;
            }
            catch (Exception e)
            {
                throw new UnknownResponseException(root, $"获取歌曲信息出错: {e} 原始内容{root}");
            }
        }
    }
}