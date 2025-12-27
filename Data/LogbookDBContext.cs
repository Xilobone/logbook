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
        /// Creates a new database context
        /// </summary>
        /// <param name="options">Context options to use</param>
        public LogbookDBContext(DbContextOptions<LogbookDBContext> options) : base(options) { }

        /// <summary>
        /// Marks some model fields as being encrypted
        /// </summary>
        /// <param name="modelBuilder">The modelbuilder that builds the model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.AccessToken)
                .HasConversion(
                    new EncryptedConverter()
                );

            modelBuilder.Entity<User>()
                .Property(u => u.RefreshToken)
                .HasConversion(
                    new EncryptedConverter()
                );
        }
    }
}