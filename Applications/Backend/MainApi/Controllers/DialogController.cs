using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.Entities;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using System.Security.Claims;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers;

[ApiController]
[Authorize]
[Route("api/dialog")]
public class DialogController : ControllerBase
{
    private readonly ILogger<DialogController> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly UserRepository _userRepository;

    public DialogController(
        ILogger<DialogController> logger,
        IMessageRepository messageRepository,
        UserRepository userRepository)
    {
        _logger = logger;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    [HttpPost("{userId}/send")]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public IActionResult Send(string userId, [FromBody] DialogSendRequest request)
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
    public IActionResult List(string userId, [FromBody] DialogListRequest request)
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

    private bool ValidateUserId(string userId)
    {
        var user = _userRepository.Get(userId);
        return user != null;
    }

    #endregion
}
