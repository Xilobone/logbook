using Logbook.Calendar;
using Logbook.Models;
using Microsoft.EntityFrameworkCore;

namespace Logbook.Data
{

    public class LogbookDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Group> Groups { get; set; }
        public LogbookDBContext(DbContextOptions<LogbookDBContext> options) : base(options) { }
    }


}