using InChambers.Core.Models.App.Constants;
using Microsoft.AspNetCore.Http;

namespace InChambers.Core.Utilities;

public static class CustomFileValidator
{
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static FileValidationResult HaveValidFile(IFormFile file)
    {
        if (file == null)
        {
            return new FileValidationResult { IsValid = true };
        }

        if (file.Length == 0)
        {
            return new FileValidationResult { IsValid = false, ErrorMessage = "No file provided or the file is empty." };
        }

        string[] allowedExtensions = DocumentTypeEnum.AllowedTypes;
        int maxFileSize = 5 * 1024 * 1024; // 5 MB

        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return new FileValidationResult { IsValid = false, ErrorMessage = "Invalid file extension." };
        }

        if (file.Length > maxFileSize)
        {
            return new FileValidationResult { IsValid = false, ErrorMessage = "File size exceeds the maximum allowed size." };
        }

        // Additional custom validation logic if needed

        return new FileValidationResult { IsValid = true, ErrorMessage = null };
    }
}