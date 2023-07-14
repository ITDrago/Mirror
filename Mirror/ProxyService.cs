using Mirror.Models;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Mirror
{
    public static class ProxyService
    {
        public static WebProxy CurrentProxy { get; set; } = null;

        private static string list = null;
        private static DateTime listLoadDate = DateTime.Now; 

        public static void ChangeProxy(MirrorDbContext db, bool reloadList = false)
        {
            if(reloadList || list == null || (DateTime.Now - listLoadDate) > TimeSpan.FromMinutes(20))
            {
                ReloadList(db.ProxyCredentials.First().ListUrl);
                listLoadDate = DateTime.Now;
            }

            /*if (CurrentProxy != null)
            {
                string currentIp = Regex.Match(CurrentProxy.Address.ToString(), "\\d+\\.\\d+\\.\\d+\\.\\d+").Value;

                if(!db.BadProxies.Any(x => x.Ip == currentIp))
                {
                    db.BadProxies.Add(new BadProxy()
                    {
                        Ip = currentIp,
                        BanDate = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }*/


            string[] lines = list.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            (string ip, int port)[] proxiesParsed = lines.Select(x => (x.Split(":")[0], int.Parse(x.Split(":")[1]))).ToArray();

            (string ip, int port) proxy = proxiesParsed[(new Random()).Next(proxiesParsed.Length)]; //proxiesParsed.First(x => !db.BadProxies.Any(b => b.Ip == x.ip));

            if(CurrentProxy != null)
                Console.WriteLine($"MyLog: Прокси {CurrentProxy.Address} забанен.");

            CurrentProxy = new WebProxy(proxy.ip, proxy.port);

            Console.WriteLine($"MyLog: Прокси заменён на {CurrentProxy.Address}.");
        }

        static void ReloadList(string url)
        {
            list = (new WebClient()).DownloadString(url);
        }
    }
}
