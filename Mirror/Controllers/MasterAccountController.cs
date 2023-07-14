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
    public class MasterAccountController : Controller
    {
        private readonly MirrorDbContext _db;

        public MasterAccountController(MirrorDbContext context)
        {
            _db = context;
        }

        // GET: MasterAccount
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

            return Redirect("/MasterAccount/Edit");
        }

        // GET: MasterAccount/Create
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

        // POST: MasterAccount/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Login,Password")] MasterAccount masterAccount)
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
                _db.Add(masterAccount);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(masterAccount);
        }

        // GET: MasterAccount/Edit/5
        public async Task<IActionResult> Edit(int? id = 1)
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

            var masterAccount = await _db.MasterAccounts.FindAsync(id);
            if (masterAccount == null)
            {
                return NotFound();
            }
            return View(masterAccount);
        }

        // POST: MasterAccount/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Login,Password")] MasterAccount masterAccount)
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

            if (id != masterAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(masterAccount);
                    await _db.SaveChangesAsync();

                    _db.MasterCookies.FirstOrDefault().Cookie = new BotBackdoor().Auth(_db.MasterAccounts.First().Login, _db.MasterAccounts.First().Password, _db);

                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MasterAccountExists(masterAccount.Id))
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
            return View(masterAccount);
        }

        // GET: MasterAccount/Delete/5
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

            var masterAccount = await _db.MasterAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (masterAccount == null)
            {
                return NotFound();
            }

            return View(masterAccount);
        }

        // POST: MasterAccount/Delete/5
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

            var masterAccount = await _db.MasterAccounts.FindAsync(id);
            _db.MasterAccounts.Remove(masterAccount);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MasterAccountExists(int id)
        {
            return _db.MasterAccounts.Any(e => e.Id == id);
        }
    }
}
