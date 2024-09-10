using SocialNetworkOtus.Shared.Database.Entities.Types;

namespace SocialNetworkOtus.Shared.Database.Entities;

public class User
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Password { get; set; }
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public City City { get; set; }
}
