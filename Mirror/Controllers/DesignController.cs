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
    public class DesignController : Controller
    {
        private readonly MirrorDbContext _db;

        public DesignController(MirrorDbContext context)
        {
            _db = context;
        }

        // GET: Design
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

            return View(await _db.CssRules.ToListAsync());
        }


        // GET: Design/Create
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

        // POST: Design/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,CssCode")] CssRule cssRule)
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
                _db.Add(cssRule);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cssRule);
        }

        // GET: Design/Edit/5
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

            var cssRule = await _db.CssRules.FindAsync(id);
            if (cssRule == null)
            {
                return NotFound();
            }
            return View(cssRule);
        }

        // POST: Design/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,CssCode")] CssRule cssRule)
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

            if (id != cssRule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(cssRule);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CssRuleExists(cssRule.Id))
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
            return View(cssRule);
        }

        // GET: Design/Delete/5
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

            var cssRule = await _db.CssRules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cssRule == null)
            {
                return NotFound();
            }

            return View(cssRule);
        }

        // POST: Design/Delete/5
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

            var cssRule = await _db.CssRules.FindAsync(id);
            _db.CssRules.Remove(cssRule);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CssRuleExists(int id)
        {
            return _db.CssRules.Any(e => e.Id == id);
        }
    }
}
