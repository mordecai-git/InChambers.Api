using InChambers.Core.Models.Utilities;
using InChambers.Core.Models.Input.AnnotatedAgreements;

namespace InChambers.Core.Interfaces;

public interface IAnnotatedAgreementService
{
    Task<Result> CreateAnnotatedAgreement(AnnotatedAgreementModel input);
    Task<Result> UpdateAnnotatedAgreement(string uid, AnnotatedAgreementModel model);
    Task<Result> RemoveAnnotatedAgreement(string uid);
    Task<Result> ListAnnotatedAgreements(AnnotatedAgreementSearchModel request);
    Task<Result> GetAnnotatedAgreement(string uid);
    Task<Result> ActivateAnnotatedAgreement(string uid);
    Task<Result> DeactivateAnnotatedAgreement(string uid);
}