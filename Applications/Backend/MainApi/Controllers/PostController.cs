using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Cache.Redis;
using SocialNetworkOtus.Shared.Database.Entities;
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
        private readonly ICacher _cacher;

        public PostController(ILogger<PostController> logger, UserRepository userRepository, ICacher cacher)
        {
            _logger = logger;
            _userRepository = userRepository;
            _cacher = cacher;
        }

        [HttpPut("feed")]
        [ProducesResponseType<PostFeedResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Feed(int offset = 0, int limit = 10)
        {
            try
            {
                if (limit > 1000)
                {
                    return BadRequest(new MessageResponse()
                    {
                        Message = "The limit is too high. Maximum 1000 elements.",
                    });
                }

                var currentUserId = GetCurrentUserId();

                var last = _cacher.GetPosts(currentUserId, 1);
                var posts = new List<PostEntity>();

                if (last != null && last.Any())
                {
                    var lag = _userRepository.GetPostLag(currentUserId, last.First().PostId);

                    if (lag == 0)
                    {
                        posts.AddRange(_cacher.GetPosts(currentUserId, limit, offset));
                        _logger.LogInformation($"Data from database: {0}. Data from cache: {posts.Count}.");
                    }
                    else
                    {
                        if (lag > 1000)
                        {
                            lag = 1000;
                        }
                        posts.AddRange(_userRepository.GetPosts(currentUserId, lag - offset, offset));
                        var newPostsCount = posts.Count;
                        _cacher.SetPosts(currentUserId, posts);
                        posts.AddRange(_cacher.GetPosts(currentUserId, limit - lag + offset, lag - offset));
                        _logger.LogInformation($"Data from database: {newPostsCount}. Data from cache: {posts.Count - newPostsCount}.");
                    }
                }
                else
                {
                    posts.AddRange(_userRepository.GetPosts(currentUserId, limit, offset));
                    _cacher.SetPosts(currentUserId, posts);
                    _logger.LogInformation($"Data from database: {posts.Count}. Data from cache: {0}.");
                }

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
                if (limit > 1000)
                {
                    return BadRequest(new MessageResponse()
                    {
                        Message = "The limit is too high. Maximum 1000 elements.",
                    });
                }

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
                var currentUserId = GetCurrentUserId();

                var post = _userRepository.CreatePost(currentUserId, request.Content);

                var friends = _userRepository.GetFriendIds(currentUserId);

                foreach (var friend in friends)
                {
                    if (_cacher.IsPosts(friend))
                    {
                        _cacher.SetPosts(friend, new List<PostEntity>() { post });
                    }
                }

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
