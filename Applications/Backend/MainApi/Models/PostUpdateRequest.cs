namespace SocialNetworkOtus.Applications.Backend.MainApi.Models
{
    public class PostUpdateRequest
    {
        public int PostId { get; set; }
        public string NewContent { get; set; }
    }
}
