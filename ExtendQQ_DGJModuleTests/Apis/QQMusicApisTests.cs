using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendQQ_DGJModule.Apis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExtendQQ_DGJModule.Apis.Tests
{
    [TestClass()]
    public class QQMusicApisTests
    {
        [TestMethod()]
        public void SearchSongAsyncTest()
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
                        ["songmid"] = new[] { "aaabbbccc" },
                        ["songtype"] = new[] { 0 },
                        ["loginflag"] = 1,
                        ["platform"] = "20"
                    }
                }
            };
            Console.WriteLine(JsonConvert.SerializeObject(data));
        }
    }
}