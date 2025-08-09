using Mapster;
using InChambers.Core.Extensions;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Input.AnnotatedAgreements;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View.AnnotatedAgreement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InChambers.Core.Services;

public class AnnotatedAgreementService : IAnnotatedAgreementService
{
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;
    private readonly IFileService _fileService;

    public AnnotatedAgreementService(InChambersContext context, UserSession userSession, IFileService fileService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public async Task<Result> CreateAnnotatedAgreement(AnnotatedAgreementModel model)
    {
        bool hubExist = await _context.AnnotatedAgreements
            .AnyAsync(sm => sm.Title.ToLower().Trim() == model.Title.ToLower().Trim());

        if (hubExist)
            return new ErrorResult("Annotated Agreement already exists");

        var annotatedAgreement = model.Adapt<AnnotatedAgreement>();
        annotatedAgreement.CreatedById = _userSession.UserId;
        annotatedAgreement.Uid = model.Title.Trim()  // trim
            .ToLower().Replace("-", "", StringComparison.OrdinalIgnoreCase) // remove hyphens
            .Replace(" ", "-", StringComparison.OrdinalIgnoreCase); // replace spaces with hyphens

        // add document
        var documentResult = await _fileService.UploadFileInternal("annotated-agreements", model.File);
        if (!documentResult.Success)
            return new ErrorResult(documentResult.Title, documentResult.Message);

        annotatedAgreement.Document = documentResult.Content;

        await _context.AddAsync(annotatedAgreement);
        await _context.SaveChangesAsync();

        return new SuccessResult(StatusCodes.Status201Created, annotatedAgreement.Adapt<AnnotatedAgreementDetailView>());
    }

    public async Task<Result> UpdateAnnotatedAgreement(string uid, AnnotatedAgreementModel model)
    {
        var annotatedAgreement = await _context.AnnotatedAgreements
            .Where(ang => ang.Uid == uid)
            .Include(ang => ang.Document)
            .FirstOrDefaultAsync();

        if (annotatedAgreement == null)
            return new ErrorResult(StatusCodes.Status404NotFound, "Item not found");

        annotatedAgreement.Title = model.Title;
        annotatedAgreement.Description = model.Description;
        annotatedAgreement.Summary = model.Summary;
        annotatedAgreement.Price = model.Price;
        annotatedAgreement.Tags = string.Join(",", model.Tags);

        if (model.File != null)
        {
            var deletedRes = await _fileService.DeleteFileInternal(annotatedAgreement.DocumentId);
            if (!deletedRes.Success)
                return new ErrorResult("Unable to delete existing file, kindly try again later");

            var documentResult = await _fileService.UploadFile("annotated-agreements", model.File);
            if (!documentResult.Success)
            {
                return new ErrorResult(documentResult.Title, documentResult.Message);
            }

            annotatedAgreement.DocumentId = documentResult.Content.Id;
        }

        annotatedAgreement.UpdatedById = _userSession.UserId;
        annotatedAgreement.UpdatedOnUtc = DateTime.UtcNow;
        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult(annotatedAgreement.Adapt<AnnotatedAgreementDetailView>())
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }

    public async Task<Result> RemoveAnnotatedAgreement(string uid)
    {
        var annotatedAgreement = await _context.AnnotatedAgreements
            .Where(ang => ang.Uid == uid)
            .FirstOrDefaultAsync();

        if (annotatedAgreement is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resource is not found.");

        _context.Remove(annotatedAgreement);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Resource is deleted successfully.")
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }

    public async Task<Result> ListAnnotatedAgreements(AnnotatedAgreementSearchModel request)
    {
        if (_userSession.IsAuthenticated && request.WithDeleted && !_userSession.IsAnyAdmin && !_userSession.IsCourseManager)
            return new ForbiddenResult();

        request.SearchQuery = !string.IsNullOrWhiteSpace(request.SearchQuery)
            ? request.SearchQuery.ToLower().Trim()
            : null;

        var annotatedAgreements = _context.AnnotatedAgreements.AsQueryable();

        // allow filters only for admin users or users who can manage courses
        annotatedAgreements = _userSession.IsAnyAdmin || _userSession.InRole(RolesConstants.ManageCourse)
            ? annotatedAgreements.Where(ang => !request.IsActive.HasValue || ang.IsActive == request.IsActive.Value)
                .Where(ang => request.WithDeleted || !ang.IsDeleted)
            : annotatedAgreements.Where(ang => ang.IsActive && !ang.IsDeleted);

        var result = await annotatedAgreements
            .Where(ang => string.IsNullOrEmpty(request.SearchQuery)
                || ang.Title.ToLower().Contains(request.SearchQuery) || ang.Summary.ToLower().Contains(request.SearchQuery))
            .ProjectToType<AnnotatedAgreementView>()
            .ToPaginatedListAsync(request.PageIndex, request.PageSize);

        return new SuccessResult(result);
    }

    public async Task<Result> GetAnnotatedAgreement(string uid)
    {
        var annotatedAgreement = _context.AnnotatedAgreements
            .Where(ang => ang.Uid == uid).AsQueryable();

        if (!_userSession.IsAnyAdmin && !_userSession.IsCourseManager)
            annotatedAgreement = annotatedAgreement.Where(ang => !ang.IsDeleted && ang.IsActive);

        var result = await annotatedAgreement
            .ProjectToType<AnnotatedAgreementDetailView>()
            .FirstOrDefaultAsync();

        if (result is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The Annotated Agreement you requested cannot be found");

        return new SuccessResult(result);
    }

    public async Task<Result> ActivateAnnotatedAgreement(string uid)
    {
        // validate Annotated Agreement
        var annotatedAgreement = await _context.AnnotatedAgreements
            .Where(sm => sm.Uid == uid)
            .FirstOrDefaultAsync();

        if (annotatedAgreement is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resourse you requested for cannot be found.");

        if (annotatedAgreement.IsActive)
            return new ErrorResult("The resource is already active.");

        if (annotatedAgreement.IsDeleted)
            return new ErrorResult("The resource is deleted.");

        annotatedAgreement.IsActive = true;
        annotatedAgreement.UpdatedById = _userSession.UserId;
        annotatedAgreement.UpdatedOnUtc = DateTime.UtcNow;

        _context.Update(annotatedAgreement);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Resource is activated successfully.")
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }

    public async Task<Result> DeactivateAnnotatedAgreement(string uid)
    {
        // validate Annotated Agreement
        var annotatedAgreement = await _context.AnnotatedAgreements
            .Where(sm => sm.Uid == uid)
            .FirstOrDefaultAsync();

        if (annotatedAgreement is null)
            return new ErrorResult(StatusCodes.Status404NotFound, "The resourse you requested for cannot be found.");

        if (!annotatedAgreement.IsActive)
            return new ErrorResult("The resource is already inactive.");

        if (annotatedAgreement.IsDeleted)
            return new ErrorResult("The resource is deleted.");

        annotatedAgreement.IsActive = false;
        annotatedAgreement.UpdatedById = _userSession.UserId;
        annotatedAgreement.UpdatedOnUtc = DateTime.UtcNow;

        _context.Update(annotatedAgreement);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult("Resource is deactivated successfully.")
            : new ErrorResult("Unable to save changes, kindly try again later.");
    }
}
