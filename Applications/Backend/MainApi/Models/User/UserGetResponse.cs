namespace SocialNetworkOtus.Applications.Backend.MainApi.Models;

public class UserGetResponse
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public bool GenderIsMale { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string City { get; set; }
    public string[] Interests { get; set; }
}
