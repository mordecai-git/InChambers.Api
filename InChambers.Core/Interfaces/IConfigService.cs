using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Interfaces;

public interface IConfigService
{
    Task<Result> ListDurations();
}