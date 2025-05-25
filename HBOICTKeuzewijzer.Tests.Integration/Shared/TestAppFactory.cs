using System.Net.Http;
using HBOICTKeuzewijzer.Api;
using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Tests.Integration.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class TestAppFactory : IDisposable
{
    public HttpClient Client { get; }
    public AppDbContext DbContext { get; }
    public string DatabaseName { get; } = Guid.NewGuid().ToString();

    private readonly WebApplicationFactory<Program> _factory;
    private readonly ServiceProvider _internalServiceProvider;

    public TestAppFactory()
    {
        // Standalone DI container to hold DbContext for assertions
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(DatabaseName));

        _internalServiceProvider = services.BuildServiceProvider();
        DbContext = _internalServiceProvider.GetRequiredService<AppDbContext>();
        DbContext.Database.EnsureCreated();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace EF context with in-memory test version
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(DatabaseName));

                    // Use fake auth scheme
                    services.AddAuthentication(FakeAuthHandler.AuthenticationScheme)
                        .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(FakeAuthHandler.AuthenticationScheme, _ => { });

                    services.PostConfigureAll<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = FakeAuthHandler.AuthenticationScheme;
                        options.DefaultChallengeScheme = FakeAuthHandler.AuthenticationScheme;
                    });

                    // Ensure DB is created inside factory scope as well
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });

        Client = _factory.CreateClient();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        _internalServiceProvider?.Dispose();
        Client?.Dispose();
        _factory?.Dispose();
    }
}
