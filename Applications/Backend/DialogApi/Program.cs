using Shared.Database.Tarantool.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DialogApi.Middlewares;
using Microsoft.Extensions.Caching.Memory;

namespace DialogApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
            //or
            //"Logging": {
            //    "LogLevel": {
            //        "Default": "Information",
            //        "Microsoft.AspNetCore": "Warning",
            //        "System.Net.Http.HttpClient": "Warning" //here
            //    }
            //},

            IConfiguration config = builder.Configuration;

            // Add services to the container.

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

            builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>(); // to automatically search all the example from assembly.

            builder.Services.AddTarantool(config);
            //builder.Services.AddPostgres(config);

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1000,
                //TrackStatistics = true,
            }));

            var app = builder.Build();

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

            app.UseMiddleware<RequestIdMiddleware>();
            app.UseMiddleware<TimeTrackingMiddleware>();

            app.Services.InitTarantool();

            app.Run();
        }
    }
}
