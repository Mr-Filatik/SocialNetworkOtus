using Microsoft.AspNetCore.Mvc;
using SocialNetworkOtus.Applications.Backend.MainApi.Models;
using SocialNetworkOtus.Shared.Database.Entities;
using SocialNetworkOtus.Shared.Database.Entities.Types;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocialNetworkOtus.Applications.Backend.MainApi.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserRepository _userRepository;

        public UserController(ILogger<UserController> logger, UserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpGet("get/{id}")]
        [ProducesResponseType<UserGetResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult GetById(string id)
        {
            try
            {
                var user = _userRepository.Get(id);

                if (user is null)
                {
                    return NotFound(new MessageResponse()
                    {
                        Message = "User not found",
                    });
                }

                return Ok(new UserGetResponse()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    SecondName = user.SecondName,
                    GenderIsMale = user.Gender == Gender.Male,
                    DateOfBirth = user.DateOfBirth,
                    City = user.City,
                    Interests = user.Interests,
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

        [HttpGet("search")]
        [ProducesResponseType<UserGetResponse[]>(StatusCodes.Status200OK)]
        [ProducesResponseType<MessageResponse>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Search([FromQuery] string firstName, string secondName)
        {
            try
            {
                var users = _userRepository.Search(firstName, secondName);

                if (!users.Any())
                {
                    return NotFound(new MessageResponse()
                    {
                        Message = "Users not found",
                    });
                }

                var response = new List<UserGetResponse>();
                foreach (var user in users)
                {
                    response.Add(new UserGetResponse()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        SecondName = user.SecondName,
                        GenderIsMale = user.Gender == Gender.Male,
                        DateOfBirth = user.DateOfBirth,
                        City = user.City,
                        Interests = user.Interests,
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse()
                {
                    Message = ex.Message, //dont return message
                });
            }
        }

        [HttpPost("register")]
        [ProducesResponseType<UserRegisterResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
        public IActionResult Register([FromBody] UserRegisterRequest value)
        {
            try
            {
                var user = new UserEntity()
                {
                    FirstName = value.FirstName,
                    SecondName = value.SecondName,
                    PasswordHash = value.Password,
                    DateOfBirth = value.DateOfBirth,
                    Gender = value.GenderIsMale ? Gender.Male : Gender.Female,
                    City = value.City,
                    Interests = value.Interests,
                };

                var userId = _userRepository.Add(user);

                return Ok(new UserRegisterResponse()
                {
                    Id = userId,
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

        // PUT api/<UserController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<UserController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
