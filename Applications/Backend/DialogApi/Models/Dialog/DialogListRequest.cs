using Swashbuckle.AspNetCore.Filters;

namespace SocialNetworkOtus.Applications.Backend.DialogApi.Models;

public class DialogListRequest
{
    public long? NewestMessageId { get; set; } = null; // null => 0
    public long? OldestMessageId { get; set; } = null; // null => 0
}

public class DialogListRequestExample : IMultipleExamplesProvider<DialogListRequest>
{
    public IEnumerable<SwaggerExample<DialogListRequest>> GetExamples()
    {
        yield return SwaggerExample.Create("Latest messages", new DialogListRequest()
        {
            NewestMessageId = null,
            OldestMessageId = null,
        });

        yield return SwaggerExample.Create("Messages newer than currect", new DialogListRequest()
        {
            NewestMessageId = 10,
            OldestMessageId = null,
        });

        yield return SwaggerExample.Create("Messages older than current", new DialogListRequest()
        {
            NewestMessageId = null,
            OldestMessageId = 10,
        });

        yield return SwaggerExample.Create("Messages in the range", new DialogListRequest()
        {
            NewestMessageId = 20,
            OldestMessageId = 10,
        });
    }
}