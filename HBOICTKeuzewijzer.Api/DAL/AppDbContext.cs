using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.DAL
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<ApplicationUserRole> ApplicationUserRole { get; set; } = null!;
        public DbSet<Category> Category { get; set; } = null!;
        public DbSet<Chat> Chat { get; set; } = null!;
        public DbSet<Message> Message { get; set; } = null!;
        public DbSet<Module> Module { get; set; } = null!;
        public DbSet<Oer> Oer { get; set; } = null!;
        public DbSet<Semester> Semester { get; set; } = null!;
        public DbSet<StudyRoute> StudyRoute { get; set; } = null!;


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
