using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Shared.Database.Abstract;
using SocialNetworkOtus.Applications.Backend.DialogApi.Models;
using SocialNetworkOtus.Shared.Database.Entities;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace SocialNetworkOtus.Applications.Backend.DialogApi.Controllers;

[ApiController]
[Authorize]
[Route("api/dialog")]
public class DialogController : ControllerBase
{
    private string _userServicePath = "";

    private readonly ILogger<DialogController> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    public DialogController(
        ILogger<DialogController> logger,
        IMessageRepository messageRepository,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    [HttpPost("{userId}/send")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Send(string userId, [FromBody] DialogSendRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var valUser = ValidateUserId(userId, token);
            var valCurrentUser = ValidateUserId(currentUserId, token);
            var res = await Task.WhenAll(valUser, valCurrentUser);

            if (res != null && res.Any(x => !x))
            {
                return BadRequest(new MessageResponse()
                {
                    Message = "User does not exist.",
                });
            }

            _messageRepository.Create(new MessageEntity()
            {
                From = currentUserId,
                To = userId,
                Text = request.Text,
            });

            return Ok(new MessageResponse()
            {
                Message = $"Message created successfully.",
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

    [HttpPost("{userId}/list")]
    [ProducesResponseType<DialogListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> List(string userId, [FromBody] DialogListRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var valUser = ValidateUserId(userId, token);
            var valCurrentUser = ValidateUserId(currentUserId, token);
            var res = await Task.WhenAll(valUser, valCurrentUser);

            if (res != null && res.Any(x => !x))
            {
                return BadRequest(new MessageResponse()
                {
                    Message = "User does not exist.",
                });
            }

            IEnumerable<MessageEntity> messages = new List<MessageEntity>();

            if (request.NewestMessageId == null && request.OldestMessageId == null)
            {
                messages = _messageRepository.GetListLatest(currentUserId, userId);
            }
            if (request.NewestMessageId != null && request.OldestMessageId == null)
            {
                messages = _messageRepository.GetListNewest(currentUserId, userId, request.NewestMessageId.Value);
            }
            if (request.NewestMessageId == null && request.OldestMessageId != null)
            {
                messages = _messageRepository.GetListOldest(currentUserId, userId, request.OldestMessageId.Value);
            }
            if (request.NewestMessageId != null && request.OldestMessageId != null)
            {
                if (request.NewestMessageId <= request.OldestMessageId)
                {
                    return BadRequest(new MessageResponse()
                    {
                        Message = "Newest message id should be more oldest message id.",
                    });
                }

                messages = _messageRepository.GetListInRange(currentUserId, userId, request.NewestMessageId.Value, request.OldestMessageId.Value);
            }

            return Ok(new DialogListResponse()
            {
                NewestMessageId = messages.Any() ? messages.First().Id : null,
                OldestMessageId = messages.Any() ? messages.Last().Id : null,
                Messages = messages.Select(e => new DialogMessage()
                {
                    Id = e.Id,
                    From = e.From,
                    To = e.To,
                    SendingTime = e.SendingTime,
                    Text = e.Text,
                }),
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

    #region Private Methods

    private string GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.Name);
        if (claim is null)
        {
            return string.Empty;
        }
        return claim.Value;
    }

    private async Task<bool> ValidateUserId(string userId, string token)
    {
        if (_memoryCache.TryGetValue<UserEntity>(userId, out var value))
        {
            //value.IsActive == true;
            //Add a consumer that will change user information if they are deleted, changed
            return true;
        }

        if (string.IsNullOrEmpty(_userServicePath))
        {
            _userServicePath = _configuration.GetConnectionString("UserService");
        }
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{_userServicePath}/get/{userId}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation($"Response status = {response.StatusCode}.");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserEntity>();
                    _memoryCache.Set(user.Id, user, new MemoryCacheEntryOptions()
                        .SetSize(1)
                        .SetPriority(CacheItemPriority.High)
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
                    return true;
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validate user by ID error.");
            return false;
        }
    }

    #endregion
}
