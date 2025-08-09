namespace InChambers.Core.Models.Input.Series;

public class SeriesSearchModel : PagingOptionModel
{
    public string SearchQuery { get; set; }
    public bool? IsActive { get; set; }
}