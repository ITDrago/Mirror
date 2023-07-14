using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Mirror.Models;
using System.Linq;

namespace Mirror.Services
{
    public static class AuthService
    {
        public static Authorization GetAuthorization(HttpRequest request, MirrorDbContext db)
        {
            Authorization auth = db.Authorizations
                .Include(x => x.User)
                .Where(x => x.Cookie == request.Cookies["Auth"])
                .Where(x => !x.Deleted)
                .OrderBy(x => x.Date)
                .LastOrDefault();

            return auth;
        }
    }
}
