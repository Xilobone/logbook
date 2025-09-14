namespace Logbook.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid EntraId { get; set; } = Guid.Empty;
        public Guid GroupId { get; set; } = Guid.Empty;
        public Group? Group { get; set; }
    }
}