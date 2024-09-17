using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SocialNetworkOtus.Shared.Database.PostgreSql;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using System.Text;

namespace SocialNetworkOtus.Applications.Backend.MainApi;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        //builder.Services.AddSingleton(builder.Configuration);

        var postgreOptions = new PostgreOptions()
        {
            ConnectionString = builder.Configuration.GetConnectionString(PostgreOptions.SectionName)
        };
        builder.Services.AddSingleton(postgreOptions);
        builder.Services.AddSingleton<PostgreDatabaseSelector>();
        builder.Services.AddSingleton<UserRepository>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        });

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        Thread.Sleep(5000);

        //initing services
        var userRepository = app.Services.GetRequiredService<UserRepository>();
        userRepository.Init();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseAuthentication();

        app.MapControllers();

        app.Run();
    }
}
