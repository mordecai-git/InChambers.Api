using Mapster;
using InChambers.Core.Extensions;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Input.SmeHub;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.SmeHub;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Services;

public class SmeHubService : ISmeHubService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly IFileService _fileService;

    public SmeHubService(InChambersContext context, UserSession userSession, IFileService fileService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public async Task<Result> CreateSmeHub(SmeHubModel model)
    {
        bool hubExist = await _context.SmeHubs
            .AnyAsync(sm => sm.Title.ToLower().Trim() == model.Title.ToLower().Trim());

        if (hubExist)
            return new ErrorResult("SME Hub already exists");

        var smeHub = model.Adapt<SmeHub>();
        smeHub.CreatedById = _userSession.UserId;
        smeHub.Uid = model.Title.Trim()  // trim
            .ToLower().Replace("-", "", StringComparison.OrdinalIgnoreCase) // remove hyphens
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase); // replace spaces with hyphens

        // add document
        var documentResult = await _fileService.UploadFileInternal("sme-hubs", model.File);
        if (!documentResult.Success)
            return new ErrorResult(documentResult.Title, documentResult.Message);

        smeHub.Document = documentResult.Content;

        await _context.AddAsync(smeHub);
        await _context.SaveChangesAsync();

        return new SuccessResult(StatusCodes.Status201Created, smeHub.Adapt<SmeHubDetailView>());
    }

    public async Task<Result> UpdateSmeHub(string uid, SmeHubModel model)
    {
        var smeHub = await _context.SmeHubs
            .Where(sh => sh.Uid == uid)
            .Include(sh => sh.Document)
            .FirstOrDefaultAsync();

        if (smeHub == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Item not found");

        smeHub.Title = model.Title;
        smeHub.Description = model.Description;
        smeHub.Summary = model.Summary;
        smeHub.TypeId = model.TypeId;
        smeHub.Price = model.Price;
        smeHub.Tags = string.Join(",", model.Tags);

        if (model.File != null)
        {
            var deletedRes = await _fileService.DeleteFileInternal(smeHub.DocumentId);
            if (!deletedRes.Success)
                return new ErrorResult("Unable to delete existing file, kindly try again later");

            var documentResult = await _fileService.UploadFile("sme-hubs", model.File);
            if (!documentResult.Success)
            {
                return new ErrorResult(documentResult.Title, documentResult.Message);
            }

            smeHub.DocumentId = documentResult.Content.Id;
        }

        smeHub.UpdatedById = _userSession.UserId;
        smeHub.UpdatedOnUtc = DateTime.UtcNow;
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(smeHub.Adapt<SmeHubDetailView>())
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }

    public async Task<Result> RemoveSmeHub(string uid)
    {
        var smeHub = await _context.SmeHubs
            .Where(sh => sh.Uid == uid)
            .FirstOrDefaultAsync();

        if (smeHub is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource is not found.");

        _context.Remove(smeHub);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Resource is deleted successfully.")
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }

    public async Task<Result> ListSmeHubs(SmeHubSearchModel request)
    {
        if (_userSession.IsAuthenticated && request.WithDeleted && !_userSession.IsAnyAdmin && !_userSession.IsCourseManager)
            return new ForbiddenResult();

        request.SearchQuery = !string.IsNullOrWhiteSpace(request.SearchQuery)
            ? request.SearchQuery.ToLower().Trim()
            : null;

        var smeHubs = _context.SmeHubs.AsQueryable();

        // allow filters only for admin users or users who can manage courses
        smeHubs = _userSession.IsAnyAdmin || _userSession.InRole(RolesConstants.ManageCourse)
            ? smeHubs.Where(sh => !request.IsActive.HasValue || sh.IsActive == request.IsActive.Value)
                .Where(sh => request.WithDeleted || !sh.IsDeleted)
            : smeHubs.Where(sh => sh.IsActive && !sh.IsDeleted);

        var result = await smeHubs
            .Where(sh => string.IsNullOrEmpty(request.SearchQuery)
                || sh.Title.ToLower().Contains(request.SearchQuery) || sh.Summary.ToLower().Contains(request.SearchQuery))
            .ProjectToType<SmeHubView>()
            .ToPaginatedListAsync(request.PageIndex, request.PageSize);

        return new SuccessResult(result);
    }

    public async Task<Result> GetSmeHub(string uid)
    {
        var smeHub = _context.SmeHubs
            .Where(sh => sh.Uid == uid).AsQueryable();

        if (!_userSession.IsAnyAdmin && !_userSession.IsCourseManager)
            smeHub = smeHub.Where(sh => !sh.IsDeleted && sh.IsActive);

        var result = await smeHub
            .ProjectToType<SmeHubDetailView>()
            .FirstOrDefaultAsync();

        if (result is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The Sme Hub you requested cannot be found");

        return new SuccessResult(result);
    }

    public async Task<Result> ListTypes()
    {
        var result = await _context.SmeHubTypes
            .ProjectToType<SmeHubTypeView>()
            .ToListAsync();

        return new SuccessResult(result);
    }

    public async Task<Result> ActivateSmeHub(string uid)
    {
        // validate sme hub
        var smeHub = await _context.SmeHubs
            .Where(sm => sm.Uid == uid)
            .FirstOrDefaultAsync();

        if (smeHub is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resourse you requested for cannot be found.");

        if (smeHub.IsActive)
            return new ErrorResult("The resource is already active.");

        if (smeHub.IsDeleted)
            return new ErrorResult("The resource is deleted.");

        smeHub.IsActive = true;
        smeHub.UpdatedById = _userSession.UserId;
        smeHub.UpdatedOnUtc = DateTime.UtcNow;

        _context.Update(smeHub);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Resource is activated successfully.")
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }

    public async Task<Result> DeactivateSmeHub(string uid)
    {
        // validate sme hub
        var smeHub = await _context.SmeHubs
            .Where(sm => sm.Uid == uid)
            .FirstOrDefaultAsync();

        if (smeHub is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resourse you requested for cannot be found.");

        if (!smeHub.IsActive)
            return new ErrorResult("The resource is already inactive.");

        if (smeHub.IsDeleted)
            return new ErrorResult("The resource is deleted.");

        smeHub.IsActive = false;
        smeHub.UpdatedById = _userSession.UserId;
        smeHub.UpdatedOnUtc = DateTime.UtcNow;

        _context.Update(smeHub);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Resource is deactivated successfully.")
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }
}
