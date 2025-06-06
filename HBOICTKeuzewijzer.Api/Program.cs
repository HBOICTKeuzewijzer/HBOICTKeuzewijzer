using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Middleware;
using HBOICTKeuzewijzer.Api.Repositories;
using HBOICTKeuzewijzer.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using HBOICTKeuzewijzer.Api.Services.StudyRouteValidation;

namespace HBOICTKeuzewijzer.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration, builder);

            var app = builder.Build();

            ConfigurePipeline(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration config, WebApplicationBuilder builder)
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
                .SetApplicationName("HBOICTKeuzewijzer");

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

                options.SPOptions.ReturnUrl = new Uri(config["Saml:ReturnUrl"] ?? throw new Exception("Saml:ReturnUrl not set"));

                options.SPOptions.PublicOrigin = new Uri(config["Saml:PublicOrigin"] ?? throw new Exception("Saml:PublicOrigin not set"));

                options.SPOptions.ModulePath = "/saml2";

                var metadataRelativePath = config["Saml:IdpMetadata"] ?? throw new Exception("Saml:IdpMetadata not set");
                var tenantId = config["AzureAd:TenantId"] ?? throw new Exception("AzureAd:TenantId not set");
                var metadataPath = Path.Combine(AppContext.BaseDirectory, metadataRelativePath);

                options.IdentityProviders.Add(new IdentityProvider(new EntityId($"https://sts.windows.net/{tenantId}/"), options.SPOptions)
                {
                    LoadMetadata = true,
                    MetadataLocation = metadataPath
                });

                options.Notifications.AuthenticationRequestCreated = (request, provider, dictionary) =>
                {
                    request.ForceAuthentication = true;
                };
            });
            
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

            services.AddScoped<IApplicationUserService, ApplicationUserService>();
            services.AddScoped<IOerUploadService, OerUploadService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ISlbRepository, SlbRepository>();
            services.AddScoped<IStudyRouteRepository, StudyRouteRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<IStudyRouteValidationService, StudyRouteValidationService>();
            
            services.AddAuthorization();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

            app.UseCors();

            app.Use(async (context, next) =>
            {
                // Prevents embedding your site in iframes (protects against clickjacking attacks)
                context.Response.Headers.Append("X-Frame-Options", "DENY");

                // Prevents browsers from MIME type sniffing (forces respect for Content-Type headers)
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

                // Prevents referrer information from being sent to other sites (strong privacy setting)
                context.Response.Headers.Append("Referrer-Policy", "no-referrer");

                // Defines which sources are allowed to load content (scripts, styles, images, etc.)
                // In this case, only allows same-origin resources (very restrictive, may break Swagger UI if used there)
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");

                await next();
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(error =>
                {
                    error.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
                    });
                });
                app.UseHsts();
            }
            
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false,
                ForwardLimit = null
            };

            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardedHeadersOptions);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseStaticFiles();
        }
    }
}
