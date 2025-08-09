using InChambers.Core.Models.Utilities;

namespace InChambers.Core.Models.View.SmeHub;

public class SmeHubDetailView : SmeHubView
{
    public int DocumentId { get; set; }
    public required string Description { get; set; }
    public List<string> Tags { get; set; } = new();

    public DocumentView Document { get; set; }
    public ReferencedUserView CreatedBy { get; set; }
    public ReferencedUserView UpdatedBy { get; set; }
}
