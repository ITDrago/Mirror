using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mirror.Models;
using Mirror.Services;

namespace Mirror.Controllers
{
    public class AdminController : Controller
    {
        private readonly MirrorDbContext _db;

        public AdminController(MirrorDbContext db)
        {
            _db = db;
        }

        // GET: AdminController
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Request.Cookies["Auth"]))
                return Redirect("/login");

            Authorization auth = AuthService.GetAuthorization(Request, _db);

            if (auth == null)
                return Redirect("/login");

            if (!auth.User.IsAdmin)
                return Redirect("/login?r=admin");

            return Redirect("/MasterAccount");
        }
    }
}
