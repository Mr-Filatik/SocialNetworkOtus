//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Memory;
//using SocialNetworkOtus.Applications.Backend.DialogApi.Models;
//using SocialNetworkOtus.Shared.Database.Entities;

//namespace DialogApi.Controllers;

//[Route("api/cache/user")]
//[Authorize]
//[ApiController]
//public class UserCacheController : ControllerBase
//{
//    private readonly ILogger<UserCacheController> _logger;
//    private readonly IMemoryCache _memoryCache;
//    private readonly IHttpClientFactory _httpClientFactory;

//    public UserCacheController(
//        ILogger<UserCacheController> logger,
//        IMemoryCache memoryCache,
//        IHttpClientFactory httpClientFactory)
//    {
//        _logger = logger;
//        _memoryCache = memoryCache;
//        _httpClientFactory = httpClientFactory;
//    }

//    [HttpPost("update")]
//    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
//    [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> CreateOrUpdate([FromBody] DialogSendRequest request)
//    {
//        try
//        {
            

//            return Ok(new MessageResponse()
//            {
//                Message = $"User created or updated successfully.",
//            });
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new ErrorResponse()
//            {
//                Message = ex.Message, //dont return message
//            });
//        }
//    }

//    [HttpPost("delete")]
//    [ProducesResponseType<MessageResponse>(StatusCodes.Status200OK)]
//    [ProducesResponseType<MessageResponse>(StatusCodes.Status400BadRequest)]
//    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> CreateOrUpdate(string userId, [FromBody] DialogSendRequest request)
//    {
//        try
//        {
            

//            return Ok(new MessageResponse()
//            {
//                Message = $"User deleted successfully.",
//            });
//        }
//        catch (Exception ex)
//        {
//            return StatusCode(500, new ErrorResponse()
//            {
//                Message = ex.Message, //dont return message
//            });
//        }
//    }
//}
