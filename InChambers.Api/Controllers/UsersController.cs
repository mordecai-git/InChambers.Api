using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.Input.Users;
using InChambers.Core.Models.View.Auth;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }


    /// <summary>
    /// Get user profile
    /// </summary>
    /// <returns></returns>
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<UserProfileView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> UserProfile()
    {
        var res = await _userService.UserProfile();
        return ProcessResponse(res);
    }

    [HttpPost("profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<UserProfileView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> UpdateProfile(ProfileModel model)
    {
        var res = await _userService.UpdateProfile(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Invite a user
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("invites")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> InviteUser(UserInvitationModel model)
    {
        var res = await _userService.InviteUser(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// Accept an invitation
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("invites/accept")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<AuthDataView>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> AcceptInvitation(AcceptInvitationModel model)
    {
        var res = await _userService.AcceptInvitation(model);
        return ProcessResponse(res);
    }

    /// <summary>
    /// List invited users
    /// </summary>
    /// <returns></returns>
    [HttpGet("invites")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult<List<InvitedUserView>>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ListInvitedUsers()
    {
        var res = await _userService.ListInvitedUsers();
        return ProcessResponse(res);
    }

    /// <summary>
    /// Resend invitation email
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("invites/{id}/resend")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    public async Task<IActionResult> ResendInvitationEmail(int id)
    {
        var res = await _userService.ResendInvitationEmail(id);
        return ProcessResponse(res);
    }
}