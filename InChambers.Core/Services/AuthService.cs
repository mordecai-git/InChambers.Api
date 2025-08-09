using Mapster;
using InChambers.Core.Constants;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Services;

public class AuthService : IAuthService
{
    private readonly InChambersContext _context;
    private readonly ITokenHandler _tokenGenerator;
    private readonly UserSession _userSession;
    private readonly IEmailService _emailService;

    public AuthService(InChambersContext context, ITokenHandler tokenGenerator, UserSession userSession,
        IEmailService emailService)
    {

        _context = context ?? throw new ArgumentNullException(nameof(context));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

    }

    public async Task<Result> Register(RegisterModel model)
    {
        // validate user with email doesn't exist
        bool userExist = await _context.Users
            .AnyAsync(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim());

        if (userExist)
            return new ErrorResult("An account with this email already exist. Please log in instead.");

        // create user object
        var user = model.Adapt<User>();
        user.HashedPassword = model.Password.HashPassword();

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RolesConstants.Customer);
        var userRole = new UserRole { User = user, Role = role };

        // save user
        await _context.AddAsync(user);
        await _context.AddAsync(userRole);

        int saved = await _context.SaveChangesAsync();
        if (saved < 1)
            return new ErrorResult("Unable to add user at the moment. Please try again");

        // send confirmation email
        string token = CodeGenerator.GenerateCode(100);
        await SaveNewCode(user.Id, token, CodePurposes.ConfirmEmail);
        await _emailService.SendConfirmEmail(user.Email, token);

        // create user token
        user.UserRoles = new List<UserRole> { userRole };
        var authData = await _tokenGenerator.GenerateJwtToken(user);

        // return user token
        if (!authData.Success)
            return new ErrorResult(authData.Message);

        return new SuccessResult(StatusCodes.Status201Created, authData.Content);
    }

    public async Task<Result> ConfirmEmail(ConfirmEmailModel model)
    {
        // validate request
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim());

        if (user == null)
            return new ErrorResult("Invalid email address.");

        if (user.EmailConfirmed)
            return new ErrorResult("Email already verified.");

        // validate token
        var today = DateTime.UtcNow;
        var code = await _context.Codes
            .FirstOrDefaultAsync(c => c.OwnerId == user.Id
                                      && c.Purpose == CodePurposes.ConfirmEmail
                                      && c.Token == model.Token
                                      && c.ExpiryDate > today
                                      && c.Used == false);
        if (code == null)
            return new ErrorResult("Invalid request, kindly request a new confirmation email.");

        // update user and token
        user.EmailConfirmed = true;
        code.Used = true;

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Email verified successfully.")
            : new ErrorResult("Unable to verify email at the moment. Please try again.");
    }

    public async Task<Result> RequestEmailConfirmation()
    {
        // validate user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _userSession.UserId);
        if (user is null)
            return new ErrorResult("Invalid request: User does not exist");

        string token = CodeGenerator.GenerateCode(100);

        bool saved = await SaveNewCode(user.Id, token, CodePurposes.ConfirmEmail);

        if (!saved)
            return new ErrorResult("Unable to send confirmation email at the moment. Please try again.");

        await _emailService.SendConfirmEmail(user.Email, token);

        return new SuccessResult("Confirmation email sent successfully.");
    }

    public async Task<Result> AuthenticateUser(LoginModel model)
    {
        model.Email = model.Email.ToLower().Trim();
        User user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email);

        if (user == null)
            return new ErrorResult("Login Failed:", "Invalid credentials.");

        if (!user.IsActive)
            return new ErrorResult("Login Failed:", "Account suspended, kindly contact the admin.");

        if (!user.HashedPassword.VerifyPassword(model.Password))
            return new ErrorResult("Login Failed:", "Invalid credentials.");

        return await _tokenGenerator.GenerateJwtToken(user);
    }

    public async Task<Result> RefreshToken(RefreshTokenModel model)
    {
        return await _tokenGenerator.RefreshJwtToken(model.RefreshToken);
    }

    public async Task<Result> Logout(string userReference)
    {
        await _tokenGenerator.InvalidateToken(userReference);
        return new SuccessResult();
    }

    public async Task<Result> RequestForPasswordReset(ForgotPasswordModel model)
    {
        string responseMessage = "If this email is associated with an account, you will receive a password reset email shortly.";

        // validate user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim());
        if (user is null)
            return new SuccessResult(responseMessage);

        string token = CodeGenerator.GenerateCode(100);

        bool saved = await SaveNewCode(user.Id, token, CodePurposes.ResetPassword);

        if (!saved)
            return new ErrorResult("Unable to send password reset email at the moment. Please try again.");

        var emailRes = await _emailService.SendPasswordResetEmail(model, token);
        if (!emailRes.Success)
            return emailRes;

        return new SuccessResult(responseMessage);
    }

    public async Task<Result> ResetPassword(ResetPasswordModel model)
    {
        // validate user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim());
        if (user is null)
            return new ErrorResult("Invalid request: User does not exist");

        // validate token
        var today = DateTime.UtcNow;
        var code = await _context.Codes
            .FirstOrDefaultAsync(c => c.OwnerId == user.Id
                && c.Purpose == CodePurposes.ResetPassword
                && c.Token == model.Token
                && c.Used == false);

        if (code == null)
            return new ErrorResult("Invalid request, kindly request a new password reset email.");

        if (code.ExpiryDate < today)
            return new ErrorResult("Password reset token has expired. Kindly request a new one.");

        // update user and token
        user.HashedPassword = model.NewPassword.HashPassword();
        code.Used = true;

        int saved = await _context.SaveChangesAsync();

        if (saved < 1)
            return new ErrorResult("Unable to reset password at the moment. Please try again.");

        // send password reset notification Email
        await _emailService.SendEmail(model.Email, "Your Password Was Just Reset - InChambers", EmailTemplates.PasswordResetNotification);

        return new SuccessResult("Password reset successful.");
    }

    #region Private Method

    private async Task<bool> SaveNewCode(int userId, string token, string purpose)
    {
        // set all existing code for this user and purpose to used
        var existingCodes = await _context.Codes
            .Where(c => c.OwnerId == userId && c.Purpose == purpose && !c.Used)
            .ToListAsync();

        foreach (var code in existingCodes)
        {
            code.Used = true;
        }

        Code newCode = new()
        {
            OwnerId = userId,
            Token = token,
            Purpose = purpose,
            ExpiryDate = DateTime.UtcNow.AddDays(1)
        };
        await _context.AddAsync(newCode);

        int saved = await _context.SaveChangesAsync();
        return saved > 0;
    }

    #endregion
}