using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShortsPoster.Models;

namespace ShortsPoster.Db
{
    public class AppDbContext : DbContext
    {
        
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserLastTags> LastTags { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

}
