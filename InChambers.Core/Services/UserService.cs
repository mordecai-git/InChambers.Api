using Mapster;
using InChambers.Core.Constants;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Configurations;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.Users;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Users;
using InChambers.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Web;

namespace InChambers.Core.Services;

public class UserService : IUserService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly IEmailService _emailService;
    private readonly BaseURLs _baseUrls;
    private readonly ITokenHandler _tokenGenerator;

    public UserService(InChambersContext context, UserSession userSession, IEmailService emailService, IOptions<AppConfig> options, ITokenHandler tokenGenerator)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));

        _baseUrls = options.Value.BaseURLs;
    }

    public async Task<Result> UserProfile()
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _userSession.UserId);

        if (user == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "User not found.");

        var userView = user.Adapt<UserProfileView>();

        return new SuccessResult(userView);
    }

    public async Task<Result> UpdateProfile(ProfileModel model)
    {
        var user = await _context.Users.FindAsync(_userSession.UserId);

        if (user is null)
            return new ErrorResult("An unkonwn error occurrend, could not find account");

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Phone = model.Phone;

        _context.Users.Update(user);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Profile updated successfully.", user.Adapt<UserProfileView>())
            : new ErrorResult("An error occurred while updating profile.");
    }

    public async Task<Result> InviteUser(UserInvitationModel model)
    {
        // validate user
        bool userExist = await _context.Users
            .AnyAsync(u => u.Email.ToLower().Trim() == model.Email.ToLower().Trim());
        if (userExist)
            return new ErrorResult("User already exist in the system");

        // validate invitation
        bool invitationExist = await _context.InvitedUsers
            .AnyAsync(i => i.Email.ToLower().Trim() == model.Email.Trim().ToLower());
        if (invitationExist)
            return new ErrorResult("User is already invited, kindly send a reminder instead.");

        string token = CodeGenerator.GenerateCode(150);

        // get and encode the url with token
        string url =
            $"{_baseUrls.AdminClient}/auth/accept-invitation?email={model.Email}&token={HttpUtility.UrlEncode(token)}";

        // save invitation
        var invitation = model.Adapt<InvitedUser>();
        invitation.Token = token;
        invitation.CreatedById = _userSession.UserId;

        await _context.AddAsync(invitation);

        int saved = await _context.SaveChangesAsync();

        if (saved < 1)
            return new ErrorResult("Unable to send invitation at the moment. Please try again.");

        // send invitation email
        var args = new Dictionary<string, string> {
            {
                "url", url
            },
            {
                "name", $"{model.FirstName} {model.LastName}"
            },
            {
                "sender_name", _userSession.Name
            }
        };
        var emailRes = await _emailService.SendEmail(model.Email, "Invitation to InChambers", EmailTemplates.Invitation, args);

        return emailRes.Success
            ? new SuccessResult("Invitation sent successfully.")
            : new SuccessResult(StatusCodes.Status201Created, "Invitation details saved successfully, but sending email failed. Kindly retry with a reminder;");
    }

    public async Task<Result> AcceptInvitation(AcceptInvitationModel model)
    {
        // validate invitation
        var invitation = await _context.InvitedUsers
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.Email.ToLower().Trim() == model.Email.ToLower().Trim()
                && i.Token == model.Token);
        if (invitation == null)
            return new ErrorResult("Invalid invitation.");

        // validate user
        bool userExist = await _context.Users
            .AnyAsync(u => invitation.IsAccepted
                || u.Email.ToLower().Trim() == model.Email.ToLower().Trim());
        if (userExist)
            return new ErrorResult("User already exist in the system");

        // set up user roles
        var roles = _context.Roles.ToList();
        var userRoles = new List<UserRole>
        {
            new UserRole
            {
                RoleId = roles.FirstOrDefault(r => r.Name == nameof(Roles.Manager)).Id,
                Role = roles.FirstOrDefault(r => r.Name == nameof(Roles.Manager))
            }
        };
        foreach (var role in roles)
        {
            if (invitation.CanManageCourses)
            {
                if (role.Name == nameof(Roles.ManageCourse))
                {
                    userRoles.Add(new UserRole
                    {
                        RoleId = role.Id,
                        Role = role
                    });
                }
            }
        }

        // create user object
        var user = new User
        {
            Email = invitation.Email,
            FirstName = invitation.FirstName,
            LastName = invitation.LastName,
            Phone = invitation.Phone,
            IsActive = true,
            UserRoles = userRoles,
            EmailConfirmed = true,
            HashedPassword = model.Password.HashPassword()
        };

        // update invitation
        invitation.IsAccepted = true;
        invitation.DateAccepted = DateTime.UtcNow;

        // save user
        await _context.AddAsync(user);

        int saved = await _context.SaveChangesAsync();
        if (saved < 1)
            return new ErrorResult("Unable to add user at the moment. Please try again");

        // TODO: sort this out later
        // send invitation accepted email
        //var args = new Dictionary<string, string>
        //{
        //    {
        //        "name", invitation.CreatedBy?.FirstName
        //    },
        //    {
        //       "member", $"{user.FirstName} {user.LastName}"
        //    },
        //    {
        //        "url", $"{_baseUrls.AdminClient}/auth/users"
        //    }
        //};
        //await _emailService.SendEmail(user.Email, "Welcome on board", EmailTemplates.InvitationAccepted, args);

        // create user token
        var authData = await _tokenGenerator.GenerateJwtToken(user);

        // return user token
        if (!authData.Success)
            return new SuccessResult(StatusCodes.Status201Created);

        return new SuccessResult(StatusCodes.Status201Created, authData.Content);
    }

    public async Task<Result> ListInvitedUsers()
    {
        var invitations = await _context.InvitedUsers
            .ProjectToType<InvitedUserView>()
            .ToListAsync();

        return new SuccessResult(invitations);
    }

    public async Task<Result> ResendInvitationEmail(int id)
    {
        var invitation = await _context.InvitedUsers
            .FindAsync(id);

        if (invitation is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resouce was not found.");

        // check if user is still pending
        if (invitation.IsAccepted)
            return new ErrorResult("This invitation has already been accepted.");

        // generate a new token
        string token = CodeGenerator.GenerateCode(150);

        // get and encode the url with token
        string url =
            $"{_baseUrls.AdminClient}/auth/accept-invitation?email={invitation.Email}&token={HttpUtility.UrlEncode(token)}";

        // save invitation
        invitation.Token = token;

        int saved = await _context.SaveChangesAsync();

        if (saved < 1)
            return new ErrorResult("Unable to send invitation at the moment. Please try again.");

        // send invitation email
        var args = new Dictionary<string, string> {
            {
                "url", url
            },
            {
                "name", $"{invitation.FirstName} {invitation.LastName}"
            },
            {
                "sender_name", _userSession.Name
            }
        };
        var emailRes = await _emailService.SendEmail(invitation.Email, "Invitation to InChambers", EmailTemplates.Invitation, args);

        return emailRes.Success
            ? new SuccessResult("Invitation sent successfully.")
            : emailRes;
    }
}
