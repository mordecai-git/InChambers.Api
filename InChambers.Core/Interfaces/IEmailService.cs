using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IEmailService
{
    Task<Result> SendConfirmEmail(string to, string token);
    Task<Result> SendPasswordResetEmail(ForgotPasswordModel model, string token);
    Task<Result> SendEmail(string to, string subject, string template, Dictionary<string, string> args = null);
}