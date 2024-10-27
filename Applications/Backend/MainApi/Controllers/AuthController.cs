using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using Swashbuckle.AspNetCore.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, UserRepository userRepository, IConfiguration configuration)
        {
            _logger = logger;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        [Route("~/api/login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request is null)
                {
                    return BadRequest(new MessageResponse()
                    {
                        Message = "Invalid data",
                    });
                }

                var user = _userRepository.Get(request.Id);
                if (user is null)
                {
                    return NotFound(new MessageResponse()
                    {
                        Message = "User not found",
                    });
                }

                var isPassword = _userRepository.VerifyPassword(request.Id, request.Password);
                if (!isPassword)
                {
                    return BadRequest(new MessageResponse()
                    {
                        Message = "Invalid data",
                    });
                }

                return Ok(new LoginResponse()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(GenerateAccessToken(request.Id)),
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse()
                {
                    Message = ex.Message, //dont return message
                });
            }
        }

        private JwtSecurityToken GenerateAccessToken(string userId)
        {
            var claims = new List<Claim>
            {
                //new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Name, userId),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])), SecurityAlgorithms.HmacSha256)
            );

            return token;

            
        }
    }
}
