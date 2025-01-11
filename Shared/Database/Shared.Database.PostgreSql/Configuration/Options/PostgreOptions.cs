namespace SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;

public class PostgreOptions
{
    public static string SectionName => typeof(PostgreOptions).Name;
    public const string MasterSectionName = "MasterConnectionString";
    public const string ReplicaSectionName = "ReplicaConnectionString";

    public string MasterConnectionString { get; set; }
    public string[] ReplicaConnectionStrings { get; set; }
}
