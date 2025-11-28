using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("gateway")]
    [Authorize]
    public class GatewayController : ControllerBase
    {
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear authentication cookies
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            
            return Ok(new { message = "Logout successful" });
        }
    }
}