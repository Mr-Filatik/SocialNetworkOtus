using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Shared.Database.Abstract;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.Entities;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers;

[ApiController]
[Authorize]
[Route("api/dialog")]
public class DialogController : ControllerBase
{
    private string _dialogServicePath = "";

    private readonly ILogger<DialogController> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly UserRepository _userRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public DialogController(
        ILogger<DialogController> logger,
        IMessageRepository messageRepository,
        UserRepository userRepository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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

            if (!ValidateUserId(userId) || !ValidateUserId(currentUserId))
            {
                return BadRequest(new MessageResponse()
                {
                    Message = "User does not exist.",
                });
            }

            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var mes = await SendRequest<MessageResponse, DialogSendRequest>($"{userId}/send", token, request);

            if (mes == null)
            {
                return BadRequest(new MessageResponse()
                {
                    Message = $"Message not created.",
                });
            }

            return Ok(mes);
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

            if (!ValidateUserId(userId) || !ValidateUserId(currentUserId))
            {
                return BadRequest(new MessageResponse()
                {
                    Message = "User does not exist.",
                });
            }

            var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var mes = await SendRequest<DialogListResponse, DialogListRequest>($"{userId}/list", token, request);

            if (mes == null)
            {
                return BadRequest(new MessageResponse()
                {
                    Message = $"Uncnown error.",
                });
            }

            return Ok(mes);
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

    private bool ValidateUserId(string userId)
    {
        var user = _userRepository.Get(userId);
        return user != null;
    }

    private async Task<TResult> SendRequest<TResult, TRequest>(string path, string token, TRequest body, Action<HttpStatusCode>? action = null)
    {
        if (string.IsNullOrEmpty(_dialogServicePath))
        {
            _dialogServicePath = _configuration.GetConnectionString("DialogService");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri($"{_dialogServicePath}/{path}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(body);

        try
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation($"Response status = {response.StatusCode}.");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var resp = await response.Content.ReadFromJsonAsync<TResult>();
                    return resp;
                }
                return default;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request error.");
            return default;
        }
    }

    #endregion
}
