using InChambers.Core.Models.App;
using InChambers.Core.Models.Input.Auth;

namespace InChambers.Core.Services;

public class DashbaordService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;

    public DashbaordService(InChambersContext context, UserSession userSession)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
    }

    //public async Task<Result> ListAll
}
