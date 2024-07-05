using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace invenory.Controllers
{
    public class ValidateTokenController : Controller
    {
        private readonly string connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;
        JwtAuthorization jwt;
        public ValidateTokenController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            jwt = new JwtAuthorization(configuration, _httpContextAccessor);
            connectionString = configuration["ConnectionStrings:PostgresDb"] ?? "";

        }
        [AllowAnonymous]
        [HttpGet]
        [Route("/api/validateToken")]
        [Authorize]
        public ActionResult validateToken()
        {
            var authorize = jwt.authorizeUser();
            return Json(authorize);

        }
    }
}
