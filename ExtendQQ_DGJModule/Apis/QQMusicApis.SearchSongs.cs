using System.Collections.Generic;
using ExtendQQ_DGJModule.Exceptions;
using ExtendQQ_DGJModule.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExtendQQ_DGJModule.Clients;
using ExtendQQ_DGJModule.Extensions;
using Newtonsoft.Json;

namespace ExtendQQ_DGJModule.Apis
{
    public static partial class QQMusicApis
    {
        /// <summary>
        /// 获取搜索接口返回值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="keyWords"></param>
        /// <param name="type"></param>
        /// <param name="pageSize"></param>
        /// <param name="offset"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static Task<JToken> SearchSongCore(HttpClient client, string keyWords, SearchType type,
            int pageSize = 5,
            int offset = 0, CancellationToken token = default)
        {
            // {
            //     "music.search.SearchCgiService": {
            //         "method": "DoSearchForQQMusicDesktop",
            //         "module": "music.search.SearchCgiService",
            //         "param": {
            //             "num_per_page": 20,
            //             "page_num": 1,
            //             "query": "陈奕迅",
            //             "search_type": 0
            //         }
            //     }
            // }
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["music.search.SearchCgiService"] = new Dictionary<string, object>
                {
                    ["method"] = "DoSearchForQQMusicDesktop",
                    ["module"] = "music.search.SearchCgiService",
                    ["param"] = new Dictionary<string, object>
                    {
                        ["num_per_page"] = pageSize,
                        ["page_num"] = offset + 1,
                        ["query"] = keyWords,
                        ["search_type"] = 0
                    }
                }
            };
            return client.PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", JsonConvert.SerializeObject(data), token)
                .GetJsonAsync(token);
        }


        /// <summary>
        /// 按给定的关键词搜索单曲
        /// </summary>
        /// <param name="client"></param>
        /// <param name="keywords">关键词</param>
        /// <param name="pageSize">本次搜索返回的实例个数上限</param>
        /// <param name="offset">偏移量</param>
        /// <param name="token"></param>
        public static async Task<SongInfo> SearchSongAsync(HttpClientv2 client, string keywords, int pageSize = 5,
            int offset = 0, CancellationToken token = default)
        {
            JObject root = (JObject)await SearchSongCore(client, keywords, SearchType.Song, pageSize, offset, token)
                .ConfigureAwait(false);
            if (root["music.search.SearchCgiService"]["code"].ToObject<int>() != 0)
            {
                throw new UnknownResponseException(root, $"搜索歌曲出错 原始内容{root}");
            }

            var songs = root["music.search.SearchCgiService"]["data"]["body"]["song"]["list"]
                .Select(SongInfo.Parse).ToArray();
            if (songs.Length == 0)
            {
                throw new UnknownResponseException(root, $"搜索歌曲出错 无搜索结果");
            }
            var url = await GetSongUrlAsync(client, songs[0].Mid, token);
            var song = songs[0];
            song.Url = url;
            return song;
        }
    }
}