namespace SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;

public class PostgreOptions
{
    public static string SectionName => typeof(PostgreOptions).Name;

    public string ConnectionString { get; set; }
}
