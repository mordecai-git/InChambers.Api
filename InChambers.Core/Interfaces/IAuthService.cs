using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IAuthService
{
    Task<Result> Register(RegisterModel model);
    Task<Result> RequestEmailConfirmation();
    Task<Result> ConfirmEmail(ConfirmEmailModel model);
    Task<Result> AuthenticateUser(LoginModel model);
    Task<Result> RefreshToken(RefreshTokenModel model);
    Task<Result> Logout(string userReference);
    Task<Result> RequestForPasswordReset(ForgotPasswordModel model);
    Task<Result> ResetPassword(ResetPasswordModel model);
}