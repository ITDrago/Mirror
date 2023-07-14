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
    public class ProxyCredentialsController : Controller
    {
        private readonly MirrorDbContext _db;

        public ProxyCredentialsController(MirrorDbContext context)
        {
            _db = context;
        }

        // GET: ProxyCredentials
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            return Redirect("/ProxyCredentials/Edit");
        }

        // GET: ProxyCredentials/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProxyCredentials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ListUrl")] ProxyCredential proxyCredential)
        {
            if (ModelState.IsValid)
            {
                _db.Add(proxyCredential);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(proxyCredential);
        }

        // GET: ProxyCredentials/Edit/5
        public async Task<IActionResult> Edit(int? id = 1)
        {
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

            var proxyCredential = await _db.ProxyCredentials.FindAsync(id);
            if (proxyCredential == null)
            {
                return NotFound();
            }
            return View(proxyCredential);
        }

        // POST: ProxyCredentials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ListUrl")] ProxyCredential proxyCredential)
        {
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            if (id != proxyCredential.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(proxyCredential);
                    await _db.SaveChangesAsync();

                    ProxyService.ChangeProxy(_db, true);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProxyCredentialExists(proxyCredential.Id))
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
            return View(proxyCredential);
        }

        // GET: ProxyCredentials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proxyCredential = await _db.ProxyCredentials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (proxyCredential == null)
            {
                return NotFound();
            }

            return View(proxyCredential);
        }

        // POST: ProxyCredentials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proxyCredential = await _db.ProxyCredentials.FindAsync(id);
            _db.ProxyCredentials.Remove(proxyCredential);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProxyCredentialExists(int id)
        {
            return _db.ProxyCredentials.Any(e => e.Id == id);
        }
    }
}
