using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConsolidatedApi.Models;
using ConsolidatedApi.Services;

namespace ConsolidatedApi.Controllers
{
    [ApiController]
    [Route("user/api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            AuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = await GenerateJwtToken(user);
            
            // Create refresh token using AuthService
            var deviceId = $"{request.DeviceType}-{request.OperatingSystem}-{request.Browser}-{DateTime.UtcNow.Ticks}";
            var refreshTokenEntity = await _authService.CreateRefreshTokenAsync(deviceId);

            // Set cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("accessToken", token, cookieOptions);
            Response.Cookies.Append("refreshToken", refreshTokenEntity.Token, cookieOptions);

            return Ok(new
            {
                message = "Login successful",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to create user", errors = result.Errors });
            }

            return Ok(new { message = "User created successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "No refresh token provided" });
            }

            try
            {
                var refreshTokenEntity = await _authService.GetRefreshTokenAsync(refreshToken);
                if (refreshTokenEntity == null)
                {
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                // Get user from the device associated with refresh token
                var user = await _userManager.FindByIdAsync(refreshTokenEntity.Device?.UserId ?? "");
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Generate new JWT token
                var newToken = await GenerateJwtToken(user);
                
                // Set new access token cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                };

                Response.Cookies.Append("accessToken", newToken, cookieOptions);

                return Ok(new { message = "Token refreshed successfully", accessToken = newToken });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Error refreshing token", error = ex.Message });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser([FromQuery] string deviceType = "", [FromQuery] string operatingSystem = "", [FromQuery] string browser = "")
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                // Add other user properties as needed
            });
        }

        [HttpPost("generate-reset-token")]
        public async Task<IActionResult> GenerateResetToken([FromBody] ResetTokenRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist
                return Ok(new { message = "If the email exists, a reset link has been sent" });
            }

            // TODO: Implement password reset token generation and email sending
            return Ok(new { message = "Reset token generated successfully" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid reset request" });
            }

            // TODO: Implement password reset logic with token validation
            var result = await _userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddPasswordAsync(user, request.NewPassword);
            }

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to reset password", errors = result.Errors });
            }

            return Ok(new { message = "Password reset successfully" });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }

            // TODO: Implement email verification logic
            return Ok(new { message = "Email verification sent" });
        }

        [HttpGet("device-confirm")]
        public async Task<IActionResult> DeviceConfirm([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            // TODO: Implement device confirmation logic
            return Ok(new { message = "Device confirmed successfully" });
        }

        [HttpPut("revoke-device/{deviceId}")]
        public async Task<IActionResult> RevokeDevice(string deviceId)
        {
            // TODO: Implement device revocation logic
            return Ok(new { message = "Device revoked successfully" });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "your-super-secret-jwt-key-here";
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ConsolidatedApi";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ConsolidatedApi";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class ResetTokenRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}