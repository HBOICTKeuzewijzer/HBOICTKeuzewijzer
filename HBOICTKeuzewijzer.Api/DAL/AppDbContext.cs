using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.DAL
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_ApplicationUser_Unique_Email");

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(e => e.ExternalId)
                .IsUnique()
                .HasDatabaseName("IX_ApplicationUser_Unique_ExternalId");

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ChatId, m.SentAt })  
                .IsUnique()
                .HasDatabaseName("IX_Message_ChatId_SentAt");
        }
    }
}
