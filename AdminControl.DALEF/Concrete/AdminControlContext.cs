using AdminControl.DALEF.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminControl.DALEF.Concrete
{
    public class AdminControlContext : DbContext
    {
        public AdminControlContext(DbContextOptions<AdminControlContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserBankCard> UserBankCards { get; set; } = null!;
        public DbSet<ActionType> ActionTypes { get; set; } = null!;
        public DbSet<AdminActionLog> AdminActionLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleID);
                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.HasIndex(e => e.Login).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Login).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);

                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AdminActionLog configuration
            modelBuilder.Entity<AdminActionLog>(entity =>
            {
                entity.HasKey(e => e.LogID);

                entity.HasOne(e => e.AdminUser)
                    .WithMany()
                    .HasForeignKey(e => e.AdminUserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TargetUser)
                    .WithMany()
                    .HasForeignKey(e => e.TargetUserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ActionType)
                    .WithMany()
                    .HasForeignKey(e => e.ActionTypeID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserBankCard configuration
            modelBuilder.Entity<UserBankCard>(entity =>
            {
                entity.HasKey(e => e.BankCardID);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
