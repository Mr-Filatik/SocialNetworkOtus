using SocialNetworkOtus.Shared.Database.Entities.Types;

namespace SocialNetworkOtus.Shared.Database.Entities;

public class UserEntity
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public CityEntity City { get; set; }
}
