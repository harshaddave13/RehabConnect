
using System.Net;
using Xunit;

namespace RehabConnect.Tests;

public class FhirAuthorizationTests
    : IClassFixture<RehabConnectWebApplicationFactory>
{
    private readonly RehabConnectWebApplicationFactory _factory;

    public FhirAuthorizationTests(
        RehabConnectWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NoAuthentication_Returns401()
    {
        using var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Remove("X-Test-User");
        client.DefaultRequestHeaders.Remove("X-Test-Role");

        var response = await client.GetAsync(
            "/fhir/Patient/patient-001");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedWithoutRole_Returns403()
    {
        using var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Remove("X-Test-Role");

        var response = await client.GetAsync(
            "/fhir/Patient/patient-001");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task ClinicianRole_Returns200()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/fhir/Patient/patient-001");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}
