using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.Entities.Types;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/friend")]
    public class FriendController : ControllerBase
    {
        private readonly ILogger<FriendController> _logger;
        private readonly UserRepository _userRepository;

        public FriendController(ILogger<FriendController> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpPut("add/{id}")]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Add(string id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var user = _userRepository.Get(id);

                if (user is null)
                {
                    return NotFound(new MessageResponse()
                    {
                        Message = "User not found",
                    });
                }

                _ = _userRepository.AddFriend(currentUserId, user.Id);

                return Ok(new MessageResponse()
                {
                    Message = $"Friend added successfully",
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

        [HttpPut("delete/{id}")] //Delete?
        [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(string id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var user = _userRepository.Get(id);

                if (user is null)
                {
                    return NotFound(new MessageResponse()
                    {
                        Message = "User not found",
                    });
                }

                _ = _userRepository.DeleteFriend(currentUserId, user.Id);

                return Ok(new MessageResponse()
                {
                    Message = "User successfully deleted",
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

        private string GetCurrentUserId()
        {
            //https://dev.to/eduardstefanescu/jwt-token-claims-in-asp-net-core-1kk8

            var claim = User.FindFirst(ClaimTypes.Name);
            if (claim is null)
            {
                return string.Empty;
            }
            return claim.Value;

            //var token = Request.Headers.Authorization.ToString();

            //var tokenHandler = new JwtSecurityTokenHandler();
            //var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            //var claimValue = securityToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        }
    }
}
