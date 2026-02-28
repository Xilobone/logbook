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

        /// <summary>
        /// Override of the default saveChanges function, also updates the timestamp fields
        /// of the saved objects, if applicable
        /// </summary>
        /// <returns>The number of state entries written to the database</returns>
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IHasTimestamps &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IHasTimestamps)entry.Entity;
                if (entry.State == EntityState.Added)
                    entity.CreatedAt = DateTime.UtcNow;

                entity.LastUpdated = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

    }
}