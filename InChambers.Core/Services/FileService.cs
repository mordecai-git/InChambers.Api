using Mapster;
using InChambers.Core.Interfaces;
using InChambers.Core.Models.App;
using InChambers.Core.Models.App.Constants;
using InChambers.Core.Models.Configurations;
using InChambers.Core.Models.Input.Auth;
using InChambers.Core.Models.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace InChambers.Core.Services;

public class FileService : IFileService
{
    private readonly FileSettings _fileSettings;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly InChambersContext _context;
    private readonly UserSession _userSession;

    public FileService(IOptions<AppConfig> appConfig, IHostEnvironment hostEnvironment, InChambersContext context,
        UserSession userSession, IHttpClientFactory clientFactory)
    {
        if (appConfig == null) throw new ArgumentNullException(nameof(appConfig));
        _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userSession = userSession ?? throw new ArgumentNullException(nameof(userSession));

        _fileSettings = appConfig.Value.FileSettings;

        // set up tinify
        TinifyAPI.Tinify.Key = appConfig.Value.TinifyKey;
    }

    public async Task<Result<DocumentView>> UploadFile(string folder, IFormFile file)
    {
        var result = await Upload(folder, file);
        if (!result.Success)
            return new ErrorResult<DocumentView>(result.Title, result.Message);

        await _context.Documents.AddAsync(result.Content);

        int saved = await _context.SaveChangesAsync();

        return saved > 0
            ? new SuccessResult<DocumentView>(result.Content.Adapt<DocumentView>())
            : new ErrorResult<DocumentView>("Saving file failed");
    }

    public async Task<Result<Document>> UploadFileInternal(string folder, IFormFile file)
        => await Upload(folder, file);

    public FileStreamResult GetFile(string folder, string fileName)
    {
        string filePath = Path.Combine(_hostEnvironment.ContentRootPath, _fileSettings.BaseFolder, folder, fileName);
        if (!File.Exists(filePath))
            return null;

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new FileStreamResult(stream, "application/octet-stream")
        {
            FileDownloadName = fileName
        };
    }

    public async Task<Result> DeleteFile(int documentId)
    {
        var result = await Delete(documentId);
        if (!result.Success)
            return new ErrorResult(result.Title, result.Message);

        int saved = await _context.SaveChangesAsync();
        return saved > 0
            ? new SuccessResult(result.Message)
            : new ErrorResult("Failed to delete file");
    }

    public async Task<Result> DeleteFileInternal(int documentId) => await Delete(documentId);

    private async Task<Result<Document>> Upload(string folder, IFormFile file)
    {
        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        string folderPath =
            Path.Combine(_hostEnvironment.ContentRootPath, _fileSettings.BaseFolder, folder);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);


        string fileType = GetDocumentType(ext);
        string fileUploadName = $"{Guid.NewGuid()}{ext}";
        string filePath = Path.Combine(folderPath, fileUploadName);
        if (fileType != DocumentTypeEnum.IMAGE)
        {
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
        else
        {
            string thumbNailFolder = Path.Combine(folderPath, "_thumbnails");
            if (!Directory.Exists(thumbNailFolder))
                Directory.CreateDirectory(thumbNailFolder);
            string thumbNailPath = Path.Combine(thumbNailFolder, fileUploadName);
            await SaveImage(filePath, thumbNailPath, file);
        }

        // save file info to database
        var document = new Document
        {
            Name = file.FileName,
            Type = fileType,
            Url = $"{folder}/{fileUploadName}",
            ThumbnailUrl = fileType == DocumentTypeEnum.IMAGE
                ? $"{folder}/thumbnails/{fileUploadName}"
                : $"static/thumbnails/{fileType}.png",
            CreatedById = _userSession.UserId
        };

        return new SuccessResult<Document>(document);
    }

    private static async Task SaveImage(string filePath, string thumbnailPath, IFormFile image)
    {
        await using (var stream = new MemoryStream())
        {
            await image.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            // Compress the image using Tinify
            var source = await TinifyAPI.Tinify.FromBuffer(stream.ToArray());

            // get thumbnail
            byte[] thumbnail = await source
                .Preserve("copyright", "creation")
                .Resize(new
                {
                    method = "fit",
                    width = 150,
                    height = 150
                }).ToBuffer();

            // compress original
            byte[] optimized = await source
                .Preserve("copyright", "creation")
                .ToBuffer();

            // save files
            await File.WriteAllBytesAsync(filePath, optimized);
            await File.WriteAllBytesAsync(thumbnailPath, thumbnail);
        }
    }

    private async Task<Result> Delete(int documentId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document is null)
            return new ErrorResult("Document not found");

        string filePath = Path.Combine(_hostEnvironment.ContentRootPath, _fileSettings.BaseFolder, document.Url);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);

            // remove thumbnail if type is an image
            if (document.Type == DocumentTypeEnum.IMAGE)
            {
                string thumbNailPath = Path.Combine(_hostEnvironment.ContentRootPath, _fileSettings.BaseFolder,
                    document.ThumbnailUrl);
                if (File.Exists(thumbNailPath))
                    File.Delete(thumbNailPath);
            }
        }
        else
        {
            return new ErrorResult("File does not exist.");
        }

        _context.Documents.Remove(document);

        return new SuccessResult("File deleted successfully.");
    }

    private static string GetDocumentType(string extension)
    {
        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
        {
            return DocumentTypeEnum.IMAGE;
        }
        else if (extension == ".pdf")
        {
            return DocumentTypeEnum.PDF;
        }
        else if (extension == ".doc" || extension == ".docx")
        {
            return DocumentTypeEnum.WORD_DOCUMENT;
        }
        else
        {
            return DocumentTypeEnum.UNKNWON;
        }
    }
}