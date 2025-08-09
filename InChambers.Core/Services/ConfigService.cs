using Mapster;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.Config;
using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Services;

public class ConfigService : IConfigService
{
    private readonly InChambersContext _context;

    public ConfigService(InChambersContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Result> ListDurations()
    {
        var durations = await _context.Durations
            .ProjectToType<DurationView>()
            .ToListAsync();

        return new SuccessResult(durations);
    }
}