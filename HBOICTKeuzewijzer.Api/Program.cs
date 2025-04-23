using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Middleware;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;

namespace HBOICTKeuzewijzer.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            ConfigurePipeline(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            // SAML Auth Setup
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Saml2Defaults.Scheme;
            })
            .AddCookie()
            .AddSaml2(options =>
            {
                options.SPOptions.EntityId = new EntityId(config["Saml:SPEntityId"] ?? throw new Exception("Saml:SPEntityId not set"));

                var metadataRelativePath = config["Saml:IdpMetadata"] ?? throw new Exception("Saml:IdpMetadata not set");
                var tenantId = config["AzureAd:TenantId"] ?? throw new Exception("AzureAd:TenantId not set");
                var metadataPath = Path.Combine(AppContext.BaseDirectory, metadataRelativePath);

                options.IdentityProviders.Add(new IdentityProvider(new EntityId($"https://sts.windows.net/{tenantId}/"), options.SPOptions)
                {
                    LoadMetadata = true,
                    MetadataLocation = metadataPath
                });

                options.SPOptions.ReturnUrl = new Uri(config["Saml:ReturnUrl"] ?? throw new Exception("Saml:ReturnUrl not set"));

                options.SPOptions.PublicOrigin = new Uri(config["Saml:PublicOrigin"] ?? throw new Exception("Saml:PublicOrigin not set"));
            });

            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // EF Core + Services
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("Default")));

            services.AddScoped<ApplicationUserService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


            services.AddAuthorization();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        private static void ConfigurePipeline(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();

            app.MapControllers();
        }
    }
}
