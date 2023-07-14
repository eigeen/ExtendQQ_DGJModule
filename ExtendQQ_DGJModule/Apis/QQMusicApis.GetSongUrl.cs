using System;
using ExtendQQ_DGJModule.Exceptions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExtendQQ_DGJModule.Extensions;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ExtendQQ_DGJModule.Apis
{
    public static partial class QQMusicApis
    {
        private static Task<JToken> GetSongUrlCore(HttpClient client, string mid,
            CancellationToken token = default)
        {
            // {
            //     "req_0": {
            //         "module": "vkey.GetVkeyServer",
            //         "method": "CgiGetVkey",
            //         "param": {
            //             "guid": "2333",
            //             "songmid": [
            //             "000MYljA3VHeeE"
            //                 ],
            //             "songtype": [
            //             0
            //                 ],
            //             "loginflag": 1,
            //             "platform": "20"
            //         }
            //     }
            var data = new Dictionary<string, object>
            {
                ["req_0"] = new Dictionary<string, object>
                {
                    ["method"] = "CgiGetVkey",
                    ["module"] = "vkey.GetVkeyServer",
                    ["param"] = new Dictionary<string, object>
                    {
                        ["guid"] = "2333",
                        ["songmid"] = new[] { mid },
                        ["songtype"] = new[] { 0 },
                        ["loginflag"] = 1,
                        ["platform"] = "20"
                    }
                }
            };

            return client.PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", JsonConvert.SerializeObject(data), token)
                .GetJsonAsync(token);
        }

        private static Task<JToken> GetSongUrlCore(HttpClient client, string mid, string fileName,
            CancellationToken token = default)
        {
            var data = new Dictionary<string, object>
            {
                ["req_0"] = new Dictionary<string, object>
                {
                    ["method"] = "CgiGetVkey",
                    ["module"] = "vkey.GetVkeyServer",
                    ["param"] = new Dictionary<string, object>
                    {
                        ["guid"] = "2333",
                        ["songmid"] = new[] { mid },
                        ["filename"] = new[] { fileName },
                        ["songtype"] = new[] { 0 },
                        ["loginflag"] = 1,
                        ["platform"] = "20"
                    }
                }
            };

            return client.PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", JsonConvert.SerializeObject(data), token)
                .GetJsonAsync(token);
        }


        public static async Task<string> GetSongUrlAsync(HttpClient client, string songId,
            CancellationToken token = default)
        {
            var root = (JObject)await GetSongUrlCore(client, songId, token).ConfigureAwait(false);
            if (root["req_0"]["code"].ToObject<int>() != 0)
            {
                throw new UnknownResponseException(root, $"获取歌曲Url出错 原始内容{root}");
            }

            try
            {
                var server = root["req_0"]["data"]["sip"].ToObject<string[]>().First();
                var pUrl = root["req_0"]["data"]["midurlinfo"][0]["purl"].ToString();
                if (string.IsNullOrEmpty(pUrl))
                {
                    throw new UnknownResponseException(root, $"无法获取到歌曲Url 原始返回值{root}");
                }

                // 更改目标后缀格式 M800 .mp3 320k 需要重新获取一次vkey
                // var repPattern = @"C400(00[\d\w]{12}).m4a";
                var MediaIdPattern = @"C400([\d\w]{14}).m4a";
                var match = Regex.Match(pUrl, MediaIdPattern);
                var mp3FileName = $"M500{match.Groups[1]}.mp3";
                // 二次获取
                root = (JObject)await GetSongUrlCore(client, songId, mp3FileName, token);
                if (root["req_0"]["code"].ToObject<int>() != 0)
                {
                    throw new UnknownResponseException(root, $"获取歌曲Url出错 原始内容{root}");
                }

                server = root["req_0"]["data"]["sip"].ToObject<string[]>().First();
                pUrl = root["req_0"]["data"]["midurlinfo"][0]["purl"].ToString();
                if (string.IsNullOrEmpty(pUrl))
                {
                    throw new UnknownResponseException(root, $"无法获取到歌曲Url 原始返回值{root}");
                }

                return server + pUrl;
            }
            catch (Exception e)
            {
                throw new UnknownResponseException(root, $"获取歌曲Url出错 解析返回值出错 原始内容{root}");
            }
        }
    }
}