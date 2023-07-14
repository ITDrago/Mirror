using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mirror.Models;
using Mirror.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirror.Controllers
{
    public class HomeController : Controller
    {
        public MirrorDbContext _db;

        public HomeController(MirrorDbContext db)
        {
            _db = db;

            /*if(_db.MasterCookies.FirstOrDefault() == null)
            {
                _db.MasterCookies.Add(new MasterCookie()
                {
                    Id = 1,
                    Cookie = new BotBackdoor().Auth(_db.MasterAccounts.First().Login, _db.MasterAccounts.First().Password, _db)
                }); 

                _db.SaveChanges();
            }

            if(ProxyService.CurrentProxy == null)
            {
                ProxyService.ChangeProxy(_db);
            }*/
        }

        public async Task<IActionResult> HandleAllRequests()
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            var masterCookies = _db.MasterCookies.FirstOrDefault();
            /*if (string.IsNullOrEmpty(masterCookies.Cookie))
            {
                return Content("Введены неверные учётные данные Mpstats.io. Обратитесь к администратору.");
            }*/

            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Models.Authorization auth = AuthService.GetAuthorization(Request, _db);

            if(auth == null)
                return Redirect("/login");

            var user = auth.User;

            if (!user.Eternal && DateTime.Now >= user.DateOfEnd)
            {
                auth.Deleted = true;

                await _db.SaveChangesAsync();

                Response.Cookies.Delete("Auth");

                return Redirect("/login");
            }

        handle:
            WebHeaderCollection headers = new WebHeaderCollection()
            {
                { "dnt", "1"},
                { "sec-ch-ua", "\"Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"YaBrowser\";v=\"23\""},
                { "sec-ch-ua-mobile", "?0"},
                { "sec-ch-ua-platform", "\"Windows\""},
                { "sec-fetch-dest", "empty"},
                { "sec-fetch-mode", "cors"},
                { "sec-fetch-site", "same-origin"},
                { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 YaBrowser/23.3.3.719 Yowser/2.5 Safari/537.36"},
                { "cookie", _db.MasterCookies.FirstOrDefault().Cookie}
            };



            string html = string.Empty;
            string url = @"https://mpstats.io" + Request.Path + Request.QueryString;
            string contentType = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers = headers;
            request.Method = Request.Method;
            request.ContentType = Request.ContentType;
            request.Proxy = new WebProxy(masterCookies.ProxyIp, masterCookies.ProxyPort);

            request.MaximumAutomaticRedirections = 3;

            if (request.Method == "POST")
            {
                CopyStreamData(Request.BodyReader.AsStream(), request.GetRequestStream());
            }

            try
            {
                using (HttpWebResponse response =  (HttpWebResponse) await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    contentType = response.Headers[HttpRequestHeader.ContentType];

                    foreach (string key in response.Headers.Keys)
                    {
                        if (key.ToLower() == "cache-control")
                        {
                            Response.Headers.Add(key, response.Headers[key]);
                        }
                    }

                    if (response.ContentType == "application/octet-stream" || response.ContentType.StartsWith("image"))
                    {
                        MemoryStream ms = new MemoryStream();

                        CopyStreamData(stream, ms);

                        return new FileContentResult(ms.ToArray(), contentType);
                    }

                    html = await reader.ReadToEndAsync();

                    if(response.ContentType.Contains("text/html"))
                    {
                        _db.Requests.Add(new Models.Request()
                        {
                            Address = Request.Path,
                            Date = DateTime.Now,
                            Ip = "1.2.3.4",
                            UserId = user.Id
                        });

                        await _db.SaveChangesAsync();

                        html = ReplaceUserBlock(html, user);

                        html = AddScreenTimer(html);

                        html = AddAllCSS(html);
                    }
                }

                
                if(html.Contains("construct_utm_uri"))
                {
                    return Content("Ожитание смены Proxy...");
                }

                return new ContentResult
                {
                    ContentType = contentType,
                    StatusCode = (int)HttpStatusCode.OK,
                    Content = html
                };

                
            }
            catch (WebException ex)
            {

                goto handle;

            }

            return Content("OOPS");
        }

        private string AddScreenTimer(string html)
        {
            string result = html;

            result = result.Replace("<body>", "<body><script>setInterval(() => {let xhr = new XMLHttpRequest(); xhr.open('GET', '/home/screentick'); xhr.send();}, 10000);</script>");

            return result;
        }

        public async Task<IActionResult> ScreenTick()
        {
            Models.Authorization auth = AuthService.GetAuthorization(Request, _db);
            var user = auth.User;

            _db.ScreenTimes.Add(new ScreenTime()
            {
                Date = DateTime.Now,
                User = user
            });

            await _db.SaveChangesAsync();

            return Content("OK");
        }

        private string AddAllCSS(string html)
        {
            string result = html;

            foreach(CssRule rule in _db.CssRules)
            {
                if(Regex.IsMatch(Request.Path, rule.Address))
                {
                    result = result.Replace("<body>", $"<body><style>{rule.CssCode}</style>");
                }
            }

            return result;
        }

        private string ReplaceUserBlock(string sourceHtml, User user)
        {
            

            return sourceHtml.Replace("</body>", @"<script>
                                                setInterval(() => 
                                                    {
                                                            var elements = document.querySelectorAll("".top-menu .popup-container"");

                                                            for (var i = 0; i < elements.length; i++) {

                                                              var newElement = document.createElement(""div"");
                                                              newElement.className = ""user-field-added"";
                                                              newElement.innerHTML = ""<i class='fa fa-user-o' aria-hidden='true'></i><div class='user-field-added-name'>" + user.Username + @"</div><a href='/logout'><i class='fa fa-sign-out' aria-hidden='true'></i></a>"";

                                                              elements[i].parentNode.replaceChild(newElement, elements[i]);
                                                            }
                                                    }, 100);
                                                
                                                </script>
                                                <style>
                                                    .user-field-added {
                                                        margin-right: 40px;
                                                        font-weight: 600;
                                                        color: #33b058;
                                                        display: flex;
                                                        align-items: center;
                                                        position: absolute;
                                                        right: 0;
                                                        margin-right: 40px;
                                                    }

                                                    .fa-user-o {
                                                        font-size: 18px !important;
                                                    }

                                                    .fa-sign-out{
                                                        font-size: 18px !important;
                                                        color: #d85b5b;
                                                    }

                                                    .user-field-added-name {
                                                        margin: 5px;
                                                    }
                                                </style>
                                                <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css"" rel=""stylesheet"">
                                            </body>");
        }

        private void CopyStreamData(Stream input, Stream output)
        {
            
                byte[] buffer = new byte[4096]; // создаем буфер для чтения данных

                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) // читаем данные из исходного stream
                {
                    output.Write(buffer, 0, bytesRead); // записываем данные в целевой stream
                }
            
        }

        
    }
}
