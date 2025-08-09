using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.View;
using InChambers.Core.Models.View.AnnotatedAgreement;

public class AnnotatedAgreementDetailView : AnnotatedAgreementView
{
    public int DocumentId { get; set; }
    public required string Description { get; set; }
    public List<string> Tags { get; set; } = new();

    public DocumentView Document { get; set; }
    public ReferencedUserView CreatedBy { get; set; }
    public ReferencedUserView UpdatedBy { get; set; }
}
