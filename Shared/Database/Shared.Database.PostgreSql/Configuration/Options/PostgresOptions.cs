namespace SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;

public class PostgresOptions
{
    public string MasterConnectionString { get; set; }
    public string[] ReplicaConnectionStrings { get; set; }
    public string[] ShardConnectionStrings { get; set; }
}
