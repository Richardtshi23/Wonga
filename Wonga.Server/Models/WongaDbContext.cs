using System;
using Microsoft.EntityFrameworkCore;


namespace Wonga.Server.Models
{
    public class WongaDbContext : DbContext
    {
        public WongaDbContext(DbContextOptions<WongaDbContext> options) : base(options) { }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
