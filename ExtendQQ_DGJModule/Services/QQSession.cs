using ExtendQQ_DGJModule.Apis;
using ExtendQQ_DGJModule.Clients;
using ExtendQQ_DGJModule.Exceptions;
using ExtendQQ_DGJModule.Models;
using ExtendQQ_DGJModule.Services;
using System;
using System.ComponentModel;
using System.Net;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ExtendQQ_DGJModule.Services
{
    public sealed class QQSession
    {
        public bool LoginStatus => !string.IsNullOrEmpty(GetCookie());

        private readonly HttpClientv2 _client;

        private readonly ConfigService _config;

        /// <summary>
        /// 初始化 <see cref="QQSession"/> 类的新实例
        /// </summary>
        public QQSession(ConfigService config, HttpClientv2 client)
        {
            _client = client;
            _config = config;
        }

        public string GetCookie()
        {
            return string.Join("; ", _client.Cookies.GetCookies(new Uri("https://u.y.qq.com/")).OfType<Cookie>().Select(p => $"{p.Name}={p.Value}"));
        }

        public void SetCookie(string cookie)
        {
            _config.Config.Cookie = cookie;
            foreach (Cookie c in _client.Cookies.GetCookies(new Uri("https://u.y.qq.com/")))
            {
                c.Expired = true;
            }
            if (!string.IsNullOrEmpty(cookie))
            {
                _client.Cookies.SetCookies(new Uri("https://u.y.qq.com/"), cookie.Replace(';', ','));
            }
        }
    }
}