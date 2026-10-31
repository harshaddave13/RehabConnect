using Hl7.Fhir.Model;
using Hl7.Fhir.Validation;

namespace RehabConnect.Api.Fhir;

public static class FhirResourceValidator
{
    public static OperationOutcome Validate(Resource resource)
    {
        var errors = resource.Validate();

        var outcome = new OperationOutcome();

        foreach (var error in errors)
        {
            outcome.Issue.Add(
                new OperationOutcome.IssueComponent
                {
                    Severity =
                        OperationOutcome.IssueSeverity.Error,

                    Code =
                        OperationOutcome.IssueType.Invalid,

                    Diagnostics = error.Message
                });
        }

        return outcome;
    }
}
