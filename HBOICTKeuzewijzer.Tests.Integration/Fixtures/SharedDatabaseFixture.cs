using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HBOICTKeuzewijzer.Api;
using Testcontainers.MsSql;
using HBOICTKeuzewijzer.Api.DAL;

namespace HBOICTKeuzewijzer.Tests.Integration.Fixtures
{
    public class SharedDatabaseFixture : IAsyncLifetime
    {
        public MsSqlContainer DbContainer { get; private set; }
        public HttpClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            DbContainer = new MsSqlBuilder()
                .WithPassword("StrongPassword(1!)")
                .Build();

            await DbContainer.StartAsync();

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove existing AppDbContext registration
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                        if (descriptor != null)
                            services.Remove(descriptor);

                        // Register test version of AppDbContext
                        services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlServer(DbContainer.GetConnectionString()));

                        // Run migrations to prepare schema
                        using var scope = services.BuildServiceProvider().CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        db.Database.Migrate();
                    });
                });

            Client = factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            await DbContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = new ServiceCollection()
                .AddDbContext<AppDbContext>(options => options.UseSqlServer(DbContainer.GetConnectionString()))
                .BuildServiceProvider()
                .CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlRawAsync("EXEC sp_msforeachtable 'DELETE FROM ?'");
        }
    }
}