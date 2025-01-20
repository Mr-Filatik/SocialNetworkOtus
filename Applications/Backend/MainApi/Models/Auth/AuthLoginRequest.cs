using Swashbuckle.AspNetCore.Filters;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Models;

public class AuthLoginRequest
{
    public string Id { get; set; }

    public string Password { get; set; }
}

public class LoginRequestExample : IMultipleExamplesProvider<AuthLoginRequest>
{
    public IEnumerable<SwaggerExample<AuthLoginRequest>> GetExamples()
    {
        yield return SwaggerExample.Create("First user", new AuthLoginRequest()
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Password = "password",
        });

        yield return SwaggerExample.Create("Second user", new AuthLoginRequest()
        {
            Id = "22222222-2222-2222-2222-222222222222",
            Password = "password",
        });

        yield return SwaggerExample.Create("Third user", new AuthLoginRequest()
        {
            Id = "33333333-3333-3333-3333-333333333333",
            Password = "password",
        });
    }
}
