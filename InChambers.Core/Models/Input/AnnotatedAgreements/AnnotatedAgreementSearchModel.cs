namespace InChambers.Core.Models.Input.AnnotatedAgreements;

public class AnnotatedAgreementSearchModel : PagingOptionModel
{
    public string SearchQuery { get; set; }
    public bool? IsActive { get; set; }
    public bool WithDeleted { get; set; } = false;
}