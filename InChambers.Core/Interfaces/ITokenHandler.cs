using InChambers.Core.Models.App;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface ITokenHandler
{
    Task<Result> GenerateJwtToken(User user);

    Task<Result> RefreshJwtToken(string refreshToken);

    Task InvalidateToken(string userReference);
    Task<bool> ValidateToken(string uid, string token, string domain);
}