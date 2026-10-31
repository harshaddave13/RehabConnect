
using Hl7.Fhir.Model;
using RehabConnect.Api.Fhir;

namespace RehabConnect.Tests;

public class FhirPatientSearchTests
{
    private const string BaseUrl = "https://localhost:7102";

    [Theory]
    [InlineData("Smith")]
    [InlineData("Smi")]
    [InlineData("smith")]
    public void Search_MatchingFamily_ReturnsPatient(
        string family)
    {
        var bundle =
            FhirPatientSearch.Search(family, BaseUrl);

        Assert.Equal(
            Bundle.BundleType.Searchset,
            bundle.Type);

        Assert.Equal(1, bundle.Total);
        Assert.Single(bundle.Entry);

        var patient =
            Assert.IsType<Patient>(
                bundle.Entry[0].Resource);

        Assert.Equal("patient-001", patient.Id);
    }

    [Fact]
    public void Search_UnknownFamily_ReturnsEmptyBundle()
    {
        var bundle =
            FhirPatientSearch.Search(
                "Williams",
                BaseUrl);

        Assert.Equal(
            Bundle.BundleType.Searchset,
            bundle.Type);

        Assert.Equal(0, bundle.Total);
        Assert.Empty(bundle.Entry);
    }

    [Fact]
    public void Search_NoFamily_ReturnsAllPatients()
    {
        var bundle =
            FhirPatientSearch.Search(null, BaseUrl);

        Assert.Equal(1, bundle.Total);
        Assert.Single(bundle.Entry);
    }

    [Fact]
    public void Search_ResultContainsCorrectFullUrl()
    {
        var bundle =
            FhirPatientSearch.Search(
                "Smith",
                BaseUrl);

        var entry = Assert.Single(bundle.Entry);

        Assert.Equal(
            $"{BaseUrl}/fhir/Patient/patient-001",
            entry.FullUrl);

        Assert.Equal(
            Bundle.SearchEntryMode.Match,
            entry.Search.Mode);
    }
}
