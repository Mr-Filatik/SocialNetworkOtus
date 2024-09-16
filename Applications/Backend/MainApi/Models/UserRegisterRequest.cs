using SocialNetworkOtus.Shared.Database.Entities.Types;
using SocialNetworkOtus.Shared.Database.Entities;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Models;

public class UserRegisterRequest
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Password { get; set; }
    public bool GenderIsMale { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string City { get; set; }
    //public CityEntity City { get; set; }
    public string[] Interests { get; set; }
}
