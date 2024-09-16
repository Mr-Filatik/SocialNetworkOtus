using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using System.Net;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserRepository _userRepository;

        public AuthController(ILogger<AuthController> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpPost]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
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
                    Token = Guid.NewGuid().ToString(),
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
    }
}
