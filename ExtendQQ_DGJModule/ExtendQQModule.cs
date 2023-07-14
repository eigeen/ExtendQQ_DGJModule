using DGJv3;
using ExtendQQ_DGJModule.Apis;
using ExtendQQ_DGJModule.Clients;
using ExtendQQ_DGJModule.Models;
using ExtendQQ_DGJModule.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using DGJSongInfo = DGJv3.SongInfo;
using SongInfo = ExtendQQ_DGJModule.Models.SongInfo;

namespace ExtendQQ_DGJModule
{
    public class ExtendQQModule : SearchModule
    {
        private readonly IDictionary<Tuple<string, Quality>, SongInfo> _downloadCache;

        private readonly QQSession _session;

        private readonly ConfigService _config;

        private readonly HttpClientv2 _client;

        public ExtendQQModule(PluginMain plugin, QQSession session, ConfigService config, HttpClientv2 client)
        {
            SetInfo("本地QQ音乐模块", "Eigeen", "dengyk2002@qq.com", plugin.PluginVer, "可以添加歌单和手动登录QQ音乐");
            _session = session;
            _config = config;
            _client = client;
            _downloadCache = new ConcurrentDictionary<Tuple<string, Quality>, SongInfo>();
        }

        public void SetLogHandler(Action<string> logHandler)
        {
            this.GetType()
                .GetProperty("_log", BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(this, logHandler);
        }

        private void AddSongItemToCache(Tuple<string, Quality> key, SongInfo value)
        {
            _downloadCache[key] = value;
            if (_downloadCache.Count > 50)
            {
                _downloadCache.Remove(_downloadCache.FirstOrDefault());
            }
        }

        protected override DownloadStatus Download(SongItem item)
        {
            throw new NotImplementedException();
        }

        protected override string GetDownloadUrl(SongItem songItem)
        {
            try
            {
                string songId = songItem.SongId;
                Quality quality = _config.Config?.Quality ?? Quality.HighQuality;
                Tuple<string, Quality> key = new Tuple<string, Quality>(songId, quality);
                if (!_downloadCache.TryGetValue(key, out SongInfo songInfo))
                {
                    songInfo = new SongInfo(songItem.SongId, songItem.SongName, new ArtistInfo[] { });
                    var url = Task.Run(() => QQMusicApis.GetSongUrlAsync(_client, songId))
                        .GetAwaiter().GetResult();
                    Log($"获取到歌曲下载链接 {url}");
                    if (!string.IsNullOrEmpty(url))
                    {
                        songInfo.Url = url;
                        // if (downloadInfo.Type.Equals("mp3", StringComparison.OrdinalIgnoreCase))
                        // {
                        AddSongItemToCache(key, songInfo);
                        return songInfo.Url;
                        // }
                        //
                        // Log(
                        //     $"由于点歌姬目前只支持播放mp3格式,当前单曲:{string.Join(";", songItem.Singers)} - {songItem.SongName} 格式:{downloadInfo.Type} 无法播放喵");
                    }
                    else
                    {
                        Log("获取下载链接失败了喵(服务器未返回下载链接)");
                    }

                    return null;
                }

                return songInfo.Url;
            }
            catch (HttpRequestException e)
            {
                Log($"获取下载链接失败了喵:{e.Message}\r\n这是由于网络原因导致获取失败, 如果多次出现, 请检查你的网络连接喵。");
            }
            catch (Exception e)
            {
                Log($"获取下载链接失败了喵:{e.Message}");
            }

            return null; // 返回null, 点歌姬会自动移除掉当前歌曲
        }

        protected override string GetLyricById(string id)
        {
            return null;
        }

        protected override DGJSongInfo Search(string keyword)
        {
            try
            {
                Log($"输入词：{keyword}");
                // 判断是id直接采用id获取
                var idPattern = @"^00[\d\w]{12}";
                var shareIdPattern = @"^[\d\w]{12}";
                var likeyShareIdPattern = @"^[\d\w]+";
                if (Regex.IsMatch(keyword, idPattern))
                {
                    Log($"{keyword} 判断为标准ID");
                    var songId = keyword; // 只是为了符合语义
                    var songInfo2 = Task.Run(() => QQMusicApis.GetSongDetailAsync(_client, songId))
                        .GetAwaiter().GetResult();
                    return new DGJSongInfo(this,
                        songInfo2.Mid,
                        songInfo2.Name,
                        songInfo2.Artists.Select(p => p.Name).ToArray());
                }

                if (Regex.IsMatch(keyword, shareIdPattern))
                {
                    Log($"{keyword} 判断为手机客户端分享ID");
                    var shareId = keyword;
                    var songId = Task.Run(() => QQMusicApis.ParsePhoneShareIdAsync(_client, shareId))
                        .GetAwaiter().GetResult();
                    var songInfo2 = Task.Run(() => QQMusicApis.GetSongDetailAsync(_client, songId))
                        .GetAwaiter().GetResult();
                    return new DGJSongInfo(this,
                        songInfo2.Mid,
                        songInfo2.Name,
                        songInfo2.Artists.Select(p => p.Name).ToArray());
                }

                // 这里单独判断是因为电脑客户端分享链接的id
                // 是随机长度的，且与手机端不一样，若返回404
                // 则继续搜索
                if (Regex.IsMatch(keyword, likeyShareIdPattern))
                {
                    Log($"{keyword} 疑似电脑客户端分享ID");
                    var shareId = keyword;
                    var songId = Task.Run(() => QQMusicApis.ParseDesktopShareIdAsync(_client, shareId))
                        .GetAwaiter().GetResult();
                    // 非空继续处理，为空则继续搜索
                    if (!string.IsNullOrEmpty(songId))
                    {
                        Log($"{keyword} 判断为电脑客户端分享ID");
                        var songInfo2 = Task.Run(() => QQMusicApis.GetSongDetailAsync(_client, songId))
                            .GetAwaiter().GetResult();
                        return new DGJSongInfo(this,
                            songInfo2.Mid,
                            songInfo2.Name,
                            songInfo2.Artists.Select(p => p.Name).ToArray());
                    }
                }

                Log($"{keyword} 判断为关键词搜索");
                var songInfo = Task.Run(() => QQMusicApis.SearchSongAsync(_client, keyword))
                    .GetAwaiter().GetResult();
                return new DGJSongInfo(this,
                    songInfo.Mid,
                    songInfo.Name,
                    songInfo.Artists.Select(p => p.Name).ToArray());
                // return null;
            }
            catch (Exception e)
            {
                Log($"搜索时发生错误: {e}");
                return null;
            }
        }

        protected override List<DGJSongInfo> GetPlaylist(string keyword)
        {
            return null;
        }

        protected override string GetLyric(SongItem songInfo)
        {
            return null;
        }
    }
}