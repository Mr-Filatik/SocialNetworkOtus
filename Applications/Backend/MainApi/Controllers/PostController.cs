using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using System.Collections.Generic;
using System.Security.Claims;

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
        [ProducesResponseType<PostFeedResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Feed(int offset = 0, int limit = 10)
        {
            try
            {
                //нужен кеш сервис
                var currentUserId = GetCurrentUserId();

                var posts = _userRepository.GetPosts(currentUserId, limit, offset);

                return Ok(new PostFeedResponse()
                {
                    Posts = posts,
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

        [HttpPut("my-feed")]
        [ProducesResponseType<PostFeedResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult MyFeed(int offset = 0, int limit = 10)
        {
            try
            {
                //нужен кеш сервис
                var currentUserId = GetCurrentUserId();

                var posts = _userRepository.GetMyPosts(currentUserId, limit, offset);

                return Ok(new PostFeedResponse()
                {
                    Posts = posts,
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

        [HttpGet("get/{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                //нужен кеш сервис
                var currentUserId = GetCurrentUserId();

                var post = _userRepository.GetPost(currentUserId, id);

                if (post is null)
                {
                    return NotFound(new MessageResponse()
                    {
                        Message = "No access to post",
                    });
                }

                return Ok(new PostGetResponse()
                {
                    Post = post,
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

        [HttpPost("create")]
        public IActionResult Create([FromBody] PostCreateRequest request)
        {
            try
            {
                //нужен кеш сервис
                var currentUserId = GetCurrentUserId();

                _ = _userRepository.CreatePost(currentUserId, request.Content);

                return Ok(new MessageResponse()
                {
                    Message = "Post created",
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

        [HttpPut("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                //нужен кеш сервис
                var currentUserId = GetCurrentUserId();

                _ = _userRepository.DeletePost(currentUserId, id);

                return Ok(new MessageResponse()
                {
                    Message = "Post deleted",
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

        [HttpPut("update")]
        public IActionResult Update([FromBody] PostUpdateRequest request)
        {
            try
            {
                //нужен кеш сервис
                var currentUserId = GetCurrentUserId();

                _ = _userRepository.UpdatePost(currentUserId, request.PostId, request.NewContent);

                return Ok(new MessageResponse()
                {
                    Message = "Post updated",
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
