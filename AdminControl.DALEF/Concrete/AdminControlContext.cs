using AdminControl.DALEF.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminControl.DALEF.Concrete
{
    public class AdminControlContext : DbContext
    {
        private readonly string? _connStr;

        // Constructor for connection string (teacher's style)
        public AdminControlContext(string connStr)
        {
            _connStr = connStr;
        }

        // Constructor for DI with options (for testing with InMemory)
        public AdminControlContext(DbContextOptions<AdminControlContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connStr))
            {
                optionsBuilder.UseSqlServer(_connStr);
            }
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserBankCard> UserBankCards { get; set; } = null!;
        public DbSet<ActionType> ActionTypes { get; set; } = null!;
        public DbSet<AdminActionLog> AdminActionLogs { get; set; } = null!;
    }
}
