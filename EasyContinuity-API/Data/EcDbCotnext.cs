using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Data
{
    public class ECDbContext : DbContext
    {
        public ECDbContext(DbContextOptions<ECDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Space> Spaces { get; set; }
        
        public DbSet<UserSpace> UserSpaces { get; set; }
        
        public DbSet<Folder> Folders { get; set; }
        
        public DbSet<Snapshot> Snapshots { get; set; }
        
        public DbSet<Attachment> Attachments { get; set; }
        
        public DbSet<Character> Characters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSpace>()
                .HasKey(u => new { u.UserId, u.SpaceId });

            modelBuilder.Entity<Folder>()
                .HasOne<Folder>()
                .WithMany()
                .HasForeignKey(f => f.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne<Folder>()
                .WithMany()
                .HasForeignKey(a => a.FolderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filters for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Space>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Folder>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<Snapshot>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Character>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Attachment>().HasQueryFilter(a => !a.IsDeleted);
        }
    }
}