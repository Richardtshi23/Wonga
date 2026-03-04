using System.ComponentModel.DataAnnotations;

namespace Wonga.Server.Models
{
    public class UserAccount
    {

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; } = new();
    }
}
