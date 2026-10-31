
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
        Assert.Equal(1, bundle.Total);

        var entry = Assert.Single(bundle.Entry);

        var observation =
            Assert.IsType<Observation>(entry.Resource);

        Assert.Equal("pain-001", observation.Id);
        Assert.Equal(
            "Patient/patient-001",
            observation.Subject.Reference);

        Assert.Equal(
            $"{BaseUrl}/fhir/Observation/pain-001",
            entry.FullUrl);
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

        Assert.Equal(1, bundle.Total);
        Assert.Single(bundle.Entry);
    }
}
