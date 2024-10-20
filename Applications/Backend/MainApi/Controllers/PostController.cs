using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/post")]
    public class PostController : ControllerBase
    {
        private readonly ILogger<PostController> _logger;
        private readonly UserRepository _userRepository;

        public PostController(ILogger<PostController> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpPut("feed")]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Feed(int offset = 0, int limit = 10) // offset = id last feed
        {
            try
            {
                if (limit < 1)
                {
                    limit = 1;
                }
                //нужен кеш сервис
                //var currentUserId = GetCurrentUserId();

                //var user = _userRepository.Get(id);

                //if (user is null)
                //{
                //    return NotFound(new MessageResponse()
                //    {
                //        Message = "User not found",
                //    });
                //}

                //_ = _userRepository.AddFriend(currentUserId, user.Id);

                return Ok(new MessageResponse()
                {
                    Message = $"Offset {offset} Limit {limit}",
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
