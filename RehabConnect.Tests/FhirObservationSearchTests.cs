
using Hl7.Fhir.Model;
using RehabConnect.Api.Fhir;

namespace RehabConnect.Tests;

public class FhirObservationSearchTests
{
    private const string BaseUrl = "https://localhost:7102";


    [Fact]
    public void Search_KnownPatient_ReturnsObservation()
    {
        var bundle = FhirObservationSearch.Search(
            "patient-001", BaseUrl);

        Assert.Equal(Bundle.BundleType.Searchset, bundle.Type);
        Assert.Equal(4, bundle.Total);
        Assert.Equal(4, bundle.Entry.Count);

        // Verify all four observations are returned.
        var observations = bundle.Entry
            .Select(entry => Assert.IsType<Observation>(entry.Resource))
            .ToList();

        Assert.Equal(
            new[] { "pain-002", "pain-003", "pain-004", "pain-001" },
            observations.Select(o => o.Id));

        // Verify every observation belongs to the correct patient.
        Assert.All(observations, observation =>
        {
            Assert.Equal(
                "Patient/patient-001",
                observation.Subject?.Reference);
        });

        // Verify the original observation is still available.
        var originalEntry = Assert.Single(
            bundle.Entry,
            entry => entry.Resource?.Id == "pain-001");

        var originalObservation =
            Assert.IsType<Observation>(originalEntry.Resource);

        Assert.Equal(4m,
            Assert.IsType<Quantity>(originalObservation.Value).Value);

        Assert.Equal(
            $"{BaseUrl}/fhir/Observation/pain-001",
            originalEntry.FullUrl);
    }


    [Fact]
    public void Search_UnknownPatient_ReturnsEmptyBundle()
    {
        var bundle = FhirObservationSearch.Search(
            "patient-999", BaseUrl);

        Assert.Equal(0, bundle.Total);
        Assert.Empty(bundle.Entry);
    }

    [Fact]
    public void Search_NoPatientFilter_ReturnsAllObservations()
    {
        var bundle = FhirObservationSearch.Search(
            null, BaseUrl);

        Assert.Equal(4, bundle.Total);
        Assert.Equal(4, bundle.Entry.Count);
    }
}
