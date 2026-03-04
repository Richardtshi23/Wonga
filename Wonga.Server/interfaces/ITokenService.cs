using Wonga.Server.Models;

namespace Wonga.Server.interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(UserAccount user, out DateTime expires);
        RefreshToken CreateRefreshToken(string ipAddress, int userId, DateTime now, int days);
    }
}
