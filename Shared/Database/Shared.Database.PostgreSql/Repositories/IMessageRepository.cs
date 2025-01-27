using SocialNetworkOtus.Shared.Database.Entities;

namespace SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

public interface IMessageRepository
{
    public void Init();
    public void Create(MessageEntity entity);
    public IEnumerable<MessageEntity> GetListLatest(string firstUser, string secondUser);
    public IEnumerable<MessageEntity> GetListNewest(string firstUser, string secondUser, long newest);
    public IEnumerable<MessageEntity> GetListOldest(string firstUser, string secondUser, long oldest);
    public IEnumerable<MessageEntity> GetListInRange(string firstUser, string secondUser, long newest, long oldest);
}
