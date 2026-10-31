
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace RehabConnect.Api.Fhir;

public static class FhirResponse
{
    private static readonly FhirJsonSerializer Serializer = new();

    public static IResult Ok(Resource resource)
    {
        var validationOutcome = FhirResourceValidator.Validate(resource);

        if (validationOutcome.Issue.Count > 0)
        {
            return Json(
                validationOutcome,
                StatusCodes.Status500InternalServerError);
        }

        return Json(resource, StatusCodes.Status200OK);
    }

    public static IResult NotFound(string message)
    {
        var outcome = new OperationOutcome
        {
            Issue =
            [
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.NotFound,
                    Diagnostics = message
                }
            ]
        };

        return Json(outcome, StatusCodes.Status404NotFound);
    }

    public static IResult Json(
        Resource resource,
        int statusCode)
    {
        var json = Serializer.SerializeToString(resource);

        return Results.Content(
            json,
            contentType: "application/fhir+json",
            statusCode: statusCode);
    }
}
