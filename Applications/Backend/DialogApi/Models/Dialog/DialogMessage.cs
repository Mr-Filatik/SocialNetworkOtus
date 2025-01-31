namespace SocialNetworkOtus.Applications.Backend.DialogApi.Models;

public class DialogMessage
{
    public long Id { get; set; }
    public string From { get; set; } //Guid
    public string To { get; set; } //Guid
    public DateTime SendingTime { get; set; }
    public string Text { get; set; }
}
