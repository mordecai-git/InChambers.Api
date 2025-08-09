using System.ComponentModel.DataAnnotations.Schema;

namespace InChambers.Core.Models.App;

public class UserContent : BaseAppModel
{
    public int UserId { get; set; }

    public int OrderId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public User User { get; set; }
    [ForeignKey("OrderId")]
    public Order Order { get; set; }
}
