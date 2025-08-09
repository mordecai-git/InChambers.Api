using InChambers.Core.Interfaces;
using InChambers.Core.Models.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Api.Controllers;

[ApiController]
[Route("api/v1/assets")]
[Authorize]
public class AssetsController : BaseController
{
    private readonly IFileService _fileService;

    public AssetsController(IFileService fileService) =>
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

    /// <summary>
    /// Upload a file for a course
    /// </summary>
    /// <param name="folder">The course folder (typically the course UID)</param>
    /// <param name="file">The file to be uploaded</param>
    /// <returns></returns>
    [HttpPost("{folder}")]
    public async Task<IActionResult> UploadAsset(string folder, IFormFile file)
    {
        var result = await _fileService.UploadFile(folder, file);
        if (result.Success)
        {
            return ProcessResponse(new SuccessResult(result.Status, result.Content));
        }
        else
        {
            return ProcessResponse(new ErrorResult(result.Status, result.Title, result.Message));
        }
    }

    /// <summary>
    /// Get a thumbnail for a file
    /// </summary>
    /// <param name="folder">The course folder (typically the course UID)</param>
    /// <param name="fileName">The file name, with its extension</param>
    /// <returns></returns>
    [HttpGet("{folder}/thumbnails/{fileName}")]
    [AllowAnonymous]
    public IActionResult GetThumbnail(string folder, string fileName) =>
        GetFile(folder, $"_thumbnails/{fileName}");

    /// <summary>
    /// Get a file for a course
    /// </summary>
    /// <param name="folder">The course folder (typically the course UID)</param>
    /// <param name="fileName">The file name, with its extension.</param>
    /// <returns></returns>
    [HttpGet("{folder}/{fileName}")]
    public IActionResult GetAsset(string folder, string fileName) =>
        GetFile(folder, fileName);

    private IActionResult GetFile(string folder, string fileName)
    {
        var result = _fileService.GetFile(folder, fileName);
        if (result != null)
            return result;

        return NotFound(new ErrorResult(StatusCodes.Status404NotFound, "File not found."));
    }
}