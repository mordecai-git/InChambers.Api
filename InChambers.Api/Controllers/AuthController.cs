using InChambers.Core.Interfaces;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Returns a user token for the user's account</returns>
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SuccessResult<AuthDataView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> SignUp(RegisterModel model)
    {
        var res = await _authService.Register(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Request email confirmation again
    /// </summary>
    /// <returns></returns>
    [HttpGet("request-email-confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> RequestEmailConfirmation()
    {
        var res = await _authService.RequestEmailConfirmation();
        return ProcessResponse(res);
    }

    /// <summary>
    /// Confirm email address
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
    {
        var res = await _authService.ConfirmEmail(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Authenticate a user
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<AuthDataView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AuthenticateUser(LoginModel model)
    {
        var res = await _authService.AuthenticateUser(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Refresh user token
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<AuthDataView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
    {
        var res = await _authService.RefreshToken(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Logout a user
    /// </summary>
    /// <param name="userReference"></param>
    /// <returns></returns>
    [HttpPost("{userReference}/logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    public async Task<IActionResult> AuthenticateUserAsync([FromRoute] string userReference)
    {
        var res = await _authService.Logout(userReference);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("request-password-reset")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> RequestPasswordReset(ForgotPasswordModel model)
    {
        var res = await _authService.RequestForPasswordReset(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Reset password
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<AuthDataView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        var res = await _authService.ResetPassword(model);
        return ProcessResponse(res);
    }
}