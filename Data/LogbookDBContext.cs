using Logbook.Models;
using Microsoft.EntityFrameworkCore;

namespace Logbook.Data
{
    /// <summary>
    /// The database context of the logbook application
    /// </summary>
    public class LogbookDBContext : DbContext
    {
        /// <summary>
        /// The set of users
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// The set of events
        /// </summary>
        public DbSet<Event> Events { get; set; }

        /// <summary>
        /// The set of groups
        /// </summary>
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// The stored users token caches
        /// </summary>
        public DbSet<TokenCache> TokenCaches { get; set; }
        /// <summary>
        /// Creates a new database context
        /// </summary>
        /// <param name="options">Context options to use</param>
        public LogbookDBContext(DbContextOptions<LogbookDBContext> options) : base(options) { }
    }
}