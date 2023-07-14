using Microsoft.EntityFrameworkCore;
using System;

namespace Mirror.Models
{
    public class MirrorDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<CssRule> CssRules { get; set; }

        public DbSet<Authorization> Authorizations { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<MasterAccount> MasterAccounts { get; set; }

        public DbSet<MasterCookie> MasterCookies { get; set; }

        public DbSet<ScreenTime> ScreenTimes { get; set; }

        public DbSet<BadProxy> BadProxies { get; set; }

        public DbSet<ProxyCredential> ProxyCredentials { get; set; }


        public MirrorDbContext(DbContextOptions<MirrorDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User()
            {
                Id = 1,
                Eternal = true,
                IsAdmin = true,
                Login = "root",
                Password = "root",
                Username = "Администратор"
            });



            modelBuilder.Entity<MasterAccount>().HasData(new MasterAccount()
            {
                Id = 1,
                Login = "managerswb@yandex.ru",
                Password = "XfBX03PC*Wd#"
            });



            modelBuilder.Entity<CssRule>().HasData(new CssRule()
            {
                Id = 1,
                Name = "Скрытие баннера Mpstats Expo",
                Address = "/.*",
                CssCode = "#mpstats-expo-pinned{display: none!important;}"
            },
            new CssRule()
            {
                Id = 2,
                Name = "Скрытие оставшегося кол-ва дней подписки",
                Address = "/.*",
                CssCode = ".has-days{display: none!important;}"
            },
            new CssRule()
            {
                Id = 3,
                Name = "Скрытие окна cookies",
                Address = "/.*",
                CssCode = ".be-content>div[style]{display: none!important;}"
            });


            modelBuilder.Entity<ProxyCredential>().HasData(new ProxyCredential()
            {
                Id = 1,
                ListUrl = "https://papaproxy.net/api/getproxy/?format=txt&type=http_ip&login=RUSJMW741V&password=9reKHnIR"
            });
        }
    }
}
