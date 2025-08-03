using CalendarAssistant.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace CalendarAssistant.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly CalendarAssistantContext _calendarAssistantContext;

        public AuthenticateController(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            CalendarAssistantContext calendarAssistantContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _calendarAssistantContext = calendarAssistantContext;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username!);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var userDetail = await _calendarAssistantContext.AspNetUsers.FindAsync(user.Id);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email!),
                    new Claim("UserId", userDetail?.UserId.ToString()!),
                    new Claim("UserName", userDetail?.UserName!),
                    new Claim("UserEmail", userDetail?.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    userId = user.Id,
                    emailId = user.Email,
                    statusCode = HttpStatusCode.OK
                });
            }
            return Unauthorized(new
            {
                statusCode = HttpStatusCode.Unauthorized
            });
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username!);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password!);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User creation failed! ${result.Errors.FirstOrDefault()?.Description}" });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }



        [HttpGet]
        [Route("isEmailAvailable")]
        public async Task<IActionResult> IsEmailAvailable(string email)
        {
            if (await _userManager.Users.AnyAsync(x => x.Email == email || x.UserName == email))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User with this email already exists" });
            }

            return Ok(new Response { Status = "Success", Message = "Email Available" });
        }

        #region "Helper Methods"
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? ""));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(12),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        #endregion
    }
}
