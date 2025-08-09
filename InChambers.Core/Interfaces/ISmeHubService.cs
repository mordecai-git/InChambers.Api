using InChambers.Core.Models.Input.SmeHub;
using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface ISmeHubService
{
    Task<Result> CreateSmeHub(SmeHubModel input);
    Task<Result> UpdateSmeHub(string uid, SmeHubModel model);
    Task<Result> RemoveSmeHub(string uid);
    Task<Result> ListSmeHubs(SmeHubSearchModel request);
    Task<Result> GetSmeHub(string uid);
    Task<Result> ListTypes();
    Task<Result> ActivateSmeHub(string uid);
    Task<Result> DeactivateSmeHub(string uid);
}