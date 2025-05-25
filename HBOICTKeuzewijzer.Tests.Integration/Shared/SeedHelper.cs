using HBOICTKeuzewijzer.Api.DAL;
using HBOICTKeuzewijzer.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HBOICTKeuzewijzer.Tests.Integration.Shared;

public static class SeedHelper
{
    public static async Task SeedAsync<TEntity>(IServiceProvider services, TEntity entity)
        where TEntity : class, IEntity
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbSet = db.Set<TEntity>();

        var exists = await dbSet.FindAsync(entity.Id);
        if (exists == null)
        {
            dbSet.Add(entity);
            await db.SaveChangesAsync();
        }
    }
}