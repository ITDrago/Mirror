using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using Mirror.Models;
using Mirror.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mirror.Controllers
{
    public class LoginController : Controller
    {
        public MirrorDbContext _db;
        private readonly Random _random;

        public LoginController(MirrorDbContext db, Random random)
        {
            _db = db;
            _random = random;
        }

        [HttpGet]
        public IActionResult Index()
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            string r = Request.QueryString.Value;

            if(r.Contains("r=admin"))
            {
                return View(new string[]
                {
                    "Для доступа к админ-панели Вам необходимо авторизоваться с помощью учётной записи администратора."
                });
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string login, string password)
        {
           
            User user = _db.Users.FirstOrDefault(x => x.Login == login && x.Password == password);

            if(user == null)
            {
                return View(new string[] 
                {
                    "Вы ввели неверный логин или пароль. Пожалуйста, проверьте введенные данные и повторите попытку."
                });
            }

            if (!user.Eternal && DateTime.Now >= user.DateOfEnd)
            {
                return View(new string[]
                {
                    "Время действия аккаунта истекло. Обратитесь к администратору для продления доступа."
                });
            }

            if (!user.Eternal && DateTime.Now >= user.DateOfEnd)
            {
                return View(new string[]
                {
                    "Время действия аккаунта истекло. Обратитесь к администратору для продления доступа."
                });
            }

            if (!user.IsAdmin && _db.Authorizations.Where(x => x.Date.Date == DateTime.Now.Date && x.UserId == user.Id).Count() >= 2)
            {
                return View(new string[]
                {
                    "Количество авторизаций на сегодняшний день исчерпано."
                });
            }

            string cookie = RandomString(50);

            if (!user.IsAdmin)
            {
                foreach (var auth in _db.Authorizations.Where(x => x.UserId == user.Id))
                {
                    auth.Deleted = true;
                }
            }

            await _db.SaveChangesAsync();
            

            _db.Authorizations.Add(new Authorization()
            {
                User = user,
                Date = DateTime.Now,
                Cookie = cookie,
                Ip = "0.1.2.3"
            });

            await _db.SaveChangesAsync();

            Response.Cookies.Append("Auth", cookie);

            return Redirect("/");
        }


        private string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }

            var finalString = new string(stringChars);

            return finalString;
        }
    }
}
