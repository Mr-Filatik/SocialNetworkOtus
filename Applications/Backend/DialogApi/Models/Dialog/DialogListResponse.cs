namespace SocialNetworkOtus.Applications.Backend.DialogApi.Models;

public class DialogListResponse
{
    public long? NewestMessageId { get; set; } = null; // null => 0
    public long? OldestMessageId { get; set; } = null; // null => 0
    public IEnumerable<DialogMessage>? Messages { get; set; } = null;
}
