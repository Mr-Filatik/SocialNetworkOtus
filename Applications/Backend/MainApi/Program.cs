using SocialNetworkOtus.Shared.Database.PostgreSql;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;

namespace SocialNetworkOtus.Applications.Backend.MainApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        var postgreOptions = new PostgreOptions()
        {
            ConnectionString = builder.Configuration.GetConnectionString(PostgreOptions.SectionName)
        };
        builder.Services.AddSingleton(postgreOptions);
        builder.Services.AddSingleton<PostgreDatabaseSelector>();
        builder.Services.AddSingleton<UserRepository>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
