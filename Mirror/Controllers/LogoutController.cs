using Microsoft.AspNetCore.Mvc;
using Mirror.Models;
using Mirror.Services;
using System.Threading.Tasks;

namespace Mirror.Controllers
{
    public class LogoutController : Controller
    {
        private readonly MirrorDbContext _db;

        public LogoutController(MirrorDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            string userId = HttpContext.User.Identity.Name;

            if (!RequestLimiter.IsRequestAllowed(userId))
            {
                return Content("Достигнут лимит запросов. Попробуйте повторить позже.");
            }
            AuthService.GetAuthorization(Request, _db).Deleted = true;

            await _db.SaveChangesAsync();

            Response.Cookies.Delete("Auth");

            return Redirect("/login");
        }
    }
}
