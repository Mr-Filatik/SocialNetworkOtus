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
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly UserRepository _userRepository;

        public LoginController(ILogger<LoginController> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpPost]
        //[ActionResultStatusCode(BadRequest)] добавить 404, 200, 400
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request is null)
            {
                return BadRequest("Invalid data");
            }

            var isUser = _userRepository.HasUser(request.Id);
            if (!isUser)
            {
                return NotFound("User not found");
            }

            var isPassword = true;
            if (!isPassword || request.Password == "error")
            {
                return BadRequest("Invalid data");
            }

            var token = new LoginResponse() { Token = Guid.NewGuid().ToString() };
            return Ok(token);
        }

        //[ResponseType(typeof(User))]
        //public HttpResponseMessage GetUser(HttpRequestMessage request, int userId, DateTime lastModifiedAtClient)
        //{
        //    var user = new DataEntities().Users.First(p => p.Id == userId);
        //    if (user.LastModified <= lastModifiedAtClient)
        //    {
        //        return new HttpResponseMessage(HttpStatusCode.NotModified);
        //    }
        //    return request.CreateResponse(HttpStatusCode.OK, user);
        //}
    }
}
