using HBOICTKeuzewijzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HBOICTKeuzewijzer.Api.DAL
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Module> Modules { get; set; } = null!;
        public DbSet<Oer> Oer { get; set; } = null!;
        public DbSet<Semester> Semesters { get; set; } = null!;
        public DbSet<StudyRoute> StudyRoutes { get; set; } = null!;
        public DbSet<Slb> Slb { get; set; } = null!;
        public DbSet<ModuleReview> ModuleReviews { get; set; }

        public DbSet<CustomModule> CustomModules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ModuleReview>()
                .HasOne(r => r.Module)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.ModuleId);

            modelBuilder.Entity<ModuleReview>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId);

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

            // Chats can be important to students so we don't want to just delete them
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.SLB)
                .WithMany()
                .HasForeignKey(c => c.SlbApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chats with no student are not relevant for SLB'ers so we delete them
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Student)
                .WithMany()
                .HasForeignKey(c => c.StudentApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Slb>()
                .HasOne(s => s.SlbApplicationUser)
                .WithMany()
                .HasForeignKey(s => s.SlbApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict); // When removing slb need to manually remove

            modelBuilder.Entity<Slb>()
                .HasOne(s => s.StudentApplicationUser)
                .WithMany()
                .HasForeignKey(s => s.StudentApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade); // When deleting studend also deletes relation with SLB

            modelBuilder.Entity<Semester>()
                .HasOne(s => s.StudyRoute)
                .WithMany(s => s.Semesters)
                .HasForeignKey(s => s.StudyRouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomModule>()
                .HasOne(c => c.Semester)
                .WithOne(s => s.CustomModule)
                .HasForeignKey<Semester>(s => s.CustomModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<StudyRoute>()
                .HasOne(s => s.ApplicationUser)
                .WithMany(a => a.StudyRoutes)
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Semester>()
                .HasOne(s => s.Module)
                .WithMany(m => m.Semesters)
                .HasForeignKey(s => s.ModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationUserRole>()
                .HasOne(a => a.ApplicationUser)
                .WithMany(a => a.ApplicationUserRoles)
                .HasForeignKey(a => a.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
