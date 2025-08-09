namespace InChambers.Core.Interfaces;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }

    int? DeletedById { get; set; }

    DateTime? DeletedOnUtc { get; set; }
}