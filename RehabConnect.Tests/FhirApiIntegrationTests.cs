
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RehabConnect.Tests;

public class FhirApiIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FhirApiIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/fhir/Patient/patient-001", "Patient")]
    [InlineData("/fhir/Observation/pain-001", "Observation")]
    public async Task Get_ExistingResource_ReturnsFhirJson(
        string url,
        string expectedResourceType)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(
            "application/fhir+json",
            response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            expectedResourceType,
            document.RootElement
                .GetProperty("resourceType")
                .GetString());
    }

    [Theory]
    [InlineData("/fhir/Patient/unknown")]
    [InlineData("/fhir/Observation/unknown")]
    public async Task Get_UnknownResource_ReturnsOperationOutcome(
        string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            "OperationOutcome",
            document.RootElement
                .GetProperty("resourceType")
                .GetString());

        Assert.NotEmpty(
            document.RootElement
                .GetProperty("issue")
                .EnumerateArray());
    }

    [Theory]
    [InlineData("/fhir/Patient?family=Smith", 1)]
    [InlineData("/fhir/Patient?family=Williams", 0)]
    [InlineData("/fhir/Observation?patient=patient-001", 1)]
    [InlineData("/fhir/Observation?patient=patient-999", 0)]
    public async Task Search_ReturnsFhirSearchBundle(
        string url,
        int expectedTotal)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(
            "application/fhir+json",
            response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal(
            "Bundle",
            root.GetProperty("resourceType").GetString());

        Assert.Equal(
            "searchset",
            root.GetProperty("type").GetString());

        Assert.Equal(
            expectedTotal,
            root.GetProperty("total").GetInt32());

        if (expectedTotal == 0)
        {
            Assert.True(
                !root.TryGetProperty("entry", out var entries) ||
                entries.GetArrayLength() == 0);
        }
        else
        {
            Assert.Equal(
                expectedTotal,
                root.GetProperty("entry").GetArrayLength());
        }
    }
}
