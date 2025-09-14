using System.Collections;

namespace Logbook.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}