using Swashbuckle.AspNetCore.Filters;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Models;

public class LoginRequest
{
    public string Id { get; set; }

    public string Password { get; set; }
}

public class LoginRequestExample : IMultipleExamplesProvider<LoginRequest>
{
    public IEnumerable<SwaggerExample<LoginRequest>> GetExamples()
    {
        yield return SwaggerExample.Create("First user", new LoginRequest()
        {
            Id = "e0c8b889-d677-4e10-9d9b-cd202a113bda",
            Password = "password",
        });

        yield return SwaggerExample.Create("Second user", new LoginRequest()
        {
            Id = "00fc8afc-6b99-43b2-962c-7a806b0816fe",
            Password = "password",
        });
    }
}
