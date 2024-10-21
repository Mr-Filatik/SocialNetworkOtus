using SocialNetworkOtus.Applications.Backend.MainApi.Controllers;
using SocialNetworkOtus.Shared.Database.Entities;

namespace SocialNetworkOtus.Applications.Backend.MainApi.Models
{
    public class PostFeedResponse
    {
        public IEnumerable<PostEntity> Posts { get; set; }
    }
}
