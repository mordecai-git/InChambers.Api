namespace InChambers.Core.Models.App.Constants;

public class DocumentTypeEnum
{
    public const string IMAGE = "Image";
    public const string PDF = "PDF";
    public const string WORD_DOCUMENT = "Word";
    public const string UNKNWON = "Unknown";
    public static readonly string[] AllowedTypes = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
}