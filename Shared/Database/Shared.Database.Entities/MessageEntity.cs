namespace SocialNetworkOtus.Shared.Database.Entities;

public class MessageEntity
{
    public long Id { get; set; }
    public string From { get; set; } //Guid
    public string To { get; set; } //Guid
    public long FromToHash { get; set; }
    public DateTime SendingTime { get; set; }
    public string Text { get; set; }
}
