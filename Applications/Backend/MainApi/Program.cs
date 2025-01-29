using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialNetworkOtus.Shared.Cache.Redis.Configuration;
using SocialNetworkOtus.Shared.Database.PostgreSql;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration.Options;
using SocialNetworkOtus.Shared.Database.PostgreSql.Repositories;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using SocialNetworkOtus.Shared.Database.PostgreSql.Configuration;
using Microsoft.Extensions.Configuration;
using SocialNetworkOtus.Shared.Event.Kafka.Configuration;

namespace SocialNetworkOtus.Applications.Backend.MainApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IConfiguration config = builder.Configuration;

        builder.Services.AddPostgres(config);

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
        builder.Services.AddSwaggerGen(options =>
        {
            // Add JWT bearer support
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            });
            // Add JWT bearer support
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        }
                    },
                    new string[]{}
                }
            });
            // Add request examples
            options.ExampleFilters(); // add this to support examples
        });

        // Add request examples
        builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>(); // to automatically search all the example from assembly.

        builder.Services.AddRedisCache();

        builder.Services.AddKafkaEvent();

        var app = builder.Build();

        Thread.Sleep(5000);

        //initing services
        app.Services.InitPostgresDatabases();

        app.Services.InitRedisCache(app.Configuration["RedisOptions:Endpoint"]);

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
            app.UseSwagger();
            app.UseSwaggerUI();
        //}

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseAuthentication();

        app.MapControllers();

        app.Run();
    }
}
