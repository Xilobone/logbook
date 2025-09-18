using System.Buffers.Text;
using System.Text;
using System.Text.Json;

namespace Logbook
{
    public class AuthState
    {
        public Guid GroupId { get; set; } = Guid.Empty;

        public static string Encode(AuthState authState)
        {
            string json = JsonSerializer.Serialize(authState);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static AuthState Decode(string state)
        {
            byte[] bytes = Convert.FromBase64String(state);
            string json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<AuthState>(json)!;

        }
    }
}