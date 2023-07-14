using Mirror.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Mirror
{
    public class BotBackdoor
    {
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 YaBrowser/23.3.3.719 Yowser/2.5 Safari/537.36";

        Dictionary<string, string> _cookie;
        public string CookieString => string.Join(';', _cookie.Select(x => $"{x.Key}={x.Value}"));

        public string Auth(string email, string password, MirrorDbContext db)
        {
            _cookie = new Dictionary<string, string>();

            int cycleId = (new Random()).Next(10000000);

            while (true)
            {
                

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://mpstats.io/");
                request.AllowAutoRedirect = false;
                request.Proxy = ProxyService.CurrentProxy;
                request.Timeout = 3000;

                request.Headers = new WebHeaderCollection()
            {
                { HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                { HttpRequestHeader.AcceptLanguage, "ru,en;q=0.9" },
                { HttpRequestHeader.CacheControl, "max-age=0" },
                { "dnt", "1" },
                { "sec-ch-ua", "\"Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"YaBrowser\";v=\"23\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "sec-fetch-dest", "document" },
                { "sec-fetch-mode", "navigate" },
                { "sec-fetch-site", "same-origin" },
                { "sec-fetch-user", "?1" },
                { "upgrade-insecure-requests", "1" },
                { HttpRequestHeader.UserAgent, userAgent },
                { HttpRequestHeader.Cookie, CookieString }
            };
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string[] cookiesSplit = response.Headers["set-cookie"].Split(";")
                                                                              .Where(x => !x.Contains("Path"))
                                                                              .Where(x => !x.Contains("Max-Age"))
                                                                              .Where(x => !x.Contains("expires"))
                                                                              .ToArray();

                        foreach (var cookieStr in cookiesSplit)
                        {
                            string cName = cookieStr.Split("=")[0];
                            string cValue = cookieStr.Split("=")[1];

                            SetCookie(cName, cValue);

                            string html = reader.ReadToEnd();

                            if (_cookie.ContainsKey("mpgd"))
                            {
                                goto norobot;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"MyLog: {cycleId} {ex}");

                    ProxyService.ChangeProxy(db);
                    continue;
                }

                var code = get_param("__js_p_", 0);
                var age = get_param("__js_p_", 1);
                var sec = get_param("__js_p_", 2);
                var disable_utm = get_param("__js_p_", 4);
                var jhash = get_jhash(code);

                SetCookie("__jhash_", jhash.ToString());
                SetCookie("__jua_", fixedEncodeURIComponent(userAgent));

                Thread.Sleep(1000);
            }

        norobot:


            string userloginCookie = Login(email, password);

            _cookie.Add("userlogin", userloginCookie);

            if(string.IsNullOrEmpty(userloginCookie))
            {
                return "";
            }

            return CookieString;
        }


        private string Login(string email, string password)
        {
            var client = new RestClient("https://mpstats.io/login");
            client.Timeout = -1;
            client.FollowRedirects = false;
            client.Proxy = ProxyService.CurrentProxy;
            var request = new RestRequest(Method.POST);

            request.AddHeader("authority", "mpstats.io");
            request.AddHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.AddHeader("accept-language", "ru,en;q=0.9");
            request.AddHeader("cache-control", "max-age=0");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cookie", CookieString);
            request.AddHeader("dnt", "1");
            request.AddHeader("origin", "https://mpstats.io");
            request.AddHeader("referer", "https://mpstats.io/login");
            request.AddHeader("sec-ch-ua", "\"Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"YaBrowser\";v=\"23\"");
            request.AddHeader("sec-ch-ua-mobile", "?0");
            request.AddHeader("sec-ch-ua-platform", "\"Windows\"");
            request.AddHeader("sec-fetch-dest", "document");
            request.AddHeader("sec-fetch-mode", "navigate");
            request.AddHeader("sec-fetch-site", "same-origin");
            request.AddHeader("sec-fetch-user", "?1");
            request.AddHeader("upgrade-insecure-requests", "1");
            client.UserAgent = userAgent;
            request.AddParameter("act", "login");
            request.AddParameter("email", email);
            request.AddParameter("password", password);
            IRestResponse response = client.Execute(request);

            try
            {
                return response.Cookies.FirstOrDefault(x => x.Name == "userlogin").Value;
            }
            catch
            {
                return "";
            }
        }

        private void SetCookie(string name, string value)
        {
            if (!_cookie.ContainsKey(name))
            {
                _cookie.Add(name, value);
            }
            else
            {
                _cookie[name] = value;
            }
        }

        private string fixedEncodeURIComponent(string str)
        {
            return Regex.Replace(Uri.EscapeDataString(str), @"[!'()*]", c => "%" + Convert.ToString(c.Value.ToCharArray()[0], 16));
        }

        private int get_jhash(int b)
        {
            var x = 123456789;
            var i = 0;
            var k = 0;

            for (i = 0; i < 1677696; i++)
            {
                x = ((x + b) ^ (x + (x % 3) + (x % 17) + b) ^ i) % 16776960;
                if (x % 117 == 0)
                {
                    k = (k + 1) % 1111;
                }
            }
            return k;
        }


        private int get_param(string store, int id)
        {
            var o = CookieString.Split(';');
            int? p = null;

            for (var i = 0; i < o.Length; i++)
            {
                if (o[i].IndexOf(store) != -1)
                {
                    var a = o[i].Split('=');
                    if (a.Length > 1)
                    {
                        var q = a[1].Split(',');
                        if (q.Length > id)
                        {
                            p = int.Parse(q[id]);
                        }
                    }
                }
            }
            if (p == null)
            {
                return 0;
            }
            return p.Value;
        }

    }
}
