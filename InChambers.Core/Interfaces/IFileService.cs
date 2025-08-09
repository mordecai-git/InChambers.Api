
using InChambers.Core.Models.App;
using InChambers.Core.Models.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InChambers.Core.Interfaces;

public interface IFileService
{
    Task<Result<DocumentView>> UploadFile(string folder, IFormFile file);
    Task<Result<Document>> UploadFileInternal(string folder, IFormFile file);
    FileStreamResult GetFile(string folder, string fileName);
    Task<Result> DeleteFile(int documentId);
    Task<Result> DeleteFileInternal(int documentId);
}