using InChambers.Core.Models.Input.Users;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IUserService
{
    Task<Result> UserProfile();
    Task<Result> UpdateProfile(ProfileModel model);
    Task<Result> InviteUser(UserInvitationModel model);
    Task<Result> AcceptInvitation(AcceptInvitationModel model);
    Task<Result> ListInvitedUsers();
    Task<Result> ResendInvitationEmail(int id);
}
