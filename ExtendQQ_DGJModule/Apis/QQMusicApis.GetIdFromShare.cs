using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ExtendQQ_DGJModule.Exceptions;
using ExtendQQ_DGJModule.Extensions;

namespace ExtendQQ_DGJModule.Apis
{
    public static partial class QQMusicApis
    {
        /// <summary>
        /// 通过分享链接的id获取真实歌曲id
        /// 此处实际未使用client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="shareId"></param>
        /// <param name="token"></param>
        /// <returns>SongId</returns>
        public static async Task<string> ParsePhoneShareIdAsync(HttpClient client, string shareId,
            CancellationToken token = default)
        {
            using HttpClient client2 = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false
            });
            shareId = shareId.TrimEnd(' ');
            var url = $"https://c6.y.qq.com/base/fcgi-bin/u?__={shareId}";
            var resp = await client2.GetAsync(url, token);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return "";
            }

            if (resp.StatusCode != HttpStatusCode.Redirect)
            {
                throw new UnknownResponseException("解析分享ID错误 响应码错误");
            }

            var loc = resp.Headers.Location.ToString();
            // 提取ID
            // https://i.y.qq.com/v8/playsong.html?ADTAG=cbshare&_wv=1&appshare=android_qq&appsongtype=1&appversion=12050008&channelId=10036163&hosteuin=oiSkoKnq7iok&openinqqmusic=1&platform=11&songmid=0020UhY82Uhzvq&type=0
            var idPattern = @"&songmid=(00[\d\w]{12})";
            var idReg = Regex.Match(loc, idPattern);
            if (idReg.Success)
            {
                return idReg.Groups[1].Value;
            }

            return "";
        }

        public static async Task<string> ParseDesktopShareIdAsync(HttpClient client, string shareId,
            CancellationToken token = default)
        {
            shareId = shareId.TrimEnd(' ');
            var url = $"https://c6.y.qq.com/base/fcgi-bin/u?__={shareId}";
            var resp = await client.GetAsync(url, token);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return "";
            }

            // 从页面中获取id关键字
            var pageIdPattern = @"""(00[\d\w]{12})""";
            var match = Regex.Match(await resp.Content.ReadAsStringAsync(), pageIdPattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "";
        }
    }
}