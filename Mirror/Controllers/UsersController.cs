using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mirror.Models;
using Mirror.Services;

namespace Mirror.Controllers
{
    public class UsersController : Controller
    {
        private readonly MirrorDbContext _db;

        public UsersController(MirrorDbContext context)
        {
            _db = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            return View(await _db.Users.ToListAsync());
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Login,Password,DateOfEnd,Eternal,IsAdmin")] User user)
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            if (ModelState.IsValid)
            {
                user.Id = _db.Users.Max(x => x.Id) + 1;

                _db.Add(user);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            if (id == null)
            {
                return NotFound();
            }

            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: Users/Statistic/5
        public async Task<IActionResult> Statistic(int? id)
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            if (id == null)
            {
                return NotFound();
            }

            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            DateTime[] dates = new DateTime[]
            {
                DateTime.Today - TimeSpan.FromDays(6),
                DateTime.Today - TimeSpan.FromDays(5),
                DateTime.Today - TimeSpan.FromDays(4),
                DateTime.Today - TimeSpan.FromDays(3),
                DateTime.Today - TimeSpan.FromDays(2),
                DateTime.Today - TimeSpan.FromDays(1),
                DateTime.Today,
            };

            (string day, int count)[] authStat = dates
                .Select(date => (date.ToString("M"), 
                         _db.Authorizations.Where(auth => auth.UserId == user.Id && auth.Date.Date == date.Date).Count())).
                         ToArray();

            (string day, int count)[] requestStat = dates
                .Select(date => (date.ToString("M"),
                         _db.Requests.Where(req => req.UserId == user.Id && req.Date.Date == date.Date).Count())).
                         ToArray();

            (string day, int count)[] screenStat = dates
                .Select(date => (date.ToString("M"),
                         _db.ScreenTimes.Where(scr => scr.UserId == user.Id && scr.Date.Date == date.Date).Count() * 10 / 60)).
                         ToArray();

            ViewBag.AuthStat = authStat;
            ViewBag.RequestStat = requestStat;
            ViewBag.ScreenStat = screenStat;

            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Login,Password,DateOfEnd,Eternal,IsAdmin")] User user)
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(user);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            if (id == null)
            {
                return NotFound();
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            var user = await _db.Users.FindAsync(id);
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _db.Users.Any(e => e.Id == id);
        }
    }
}
