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
    public string DatabaseName { get; } = Guid.NewGuid().ToString();

    private readonly WebApplicationFactory<Program> _factory;

    public IServiceProvider Services => _factory.Services;

    public TestAppFactory()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(DatabaseName));

                    services.AddAuthentication(FakeAuthHandler.AuthenticationScheme)
                        .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(
                            FakeAuthHandler.AuthenticationScheme, _ => { });

                    services.PostConfigureAll<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = FakeAuthHandler.AuthenticationScheme;
                        options.DefaultChallengeScheme = FakeAuthHandler.AuthenticationScheme;
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });

        Client = _factory.CreateClient();
    }

    public AppDbContext CreateDbContext()
    {
        return Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public void Dispose()
    {
        Client?.Dispose();
        _factory?.Dispose();
    }
}
