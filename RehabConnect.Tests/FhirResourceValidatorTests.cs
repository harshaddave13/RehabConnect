
using Hl7.Fhir.Model;
using RehabConnect.Api.Fhir;

namespace RehabConnect.Tests;

public class FhirResourceValidatorTests
{
    [Fact]
    public void ValidPatient_ShouldHaveNoValidationIssues()
    {
        // Arrange
        var patient = FhirSampleData.CreatePatient();

        // Act
        var outcome = FhirResourceValidator.Validate(patient);

        // Assert
        Assert.Empty(outcome.Issue);
    }

    [Fact]
    public void ValidObservation_ShouldHaveNoValidationIssues()
    {
        // Arrange
        var observation =
            FhirSampleData.CreatePainObservation();

        // Act
        var outcome =
            FhirResourceValidator.Validate(observation);

        // Assert
        Assert.Empty(outcome.Issue);
    }

    [Fact]
    public void ObservationMissingRequiredFields_ShouldFailValidation()
    {
        // Arrange
        var observation = new Observation
        {
            Id = "invalid-observation"
        };

        // Act
        var outcome =
            FhirResourceValidator.Validate(observation);

        // Assert
        Assert.NotEmpty(outcome.Issue);

        Assert.All(outcome.Issue, issue =>
            Assert.Equal(
                OperationOutcome.IssueSeverity.Error,
                issue.Severity));
    }
}
