using Hl7.Fhir.Serialization;
using RehabConnect.Api.Fhir;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddAuthorization(options => 
{ 
    options.AddPolicy("ClinicalRead", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Clinician", "Admin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));
});


// Register OpenAPI documentation services
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI is available in development only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Application health endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        application = "RehabConnect",
        version = "1.0",
        status = "Running"
    });
});

// Synthetic FHIR Patient endpoint
app.MapGet("/fhir/Patient/{id}", (string id) =>
{
    if (id != "patient-001")
    {
        return FhirResponse.NotFound($"Patient '{id}' was not found.");
    }

    var patient = FhirSampleData.CreatePatient();

    return FhirResponse.Ok(patient);
})
.RequireAuthorization("ClinicalRead")
.WithName("GetFhirPatient")
.WithSummary("Retrieve a synthetic FHIR patient");


app.MapGet("/fhir/Observation/{id}", (string id) =>
{
    if (id != "pain-001")
    {
        return FhirResponse.NotFound($"Observation '{id}' was not found.");
    }

    var observation = FhirSampleData.CreatePainObservation();

    return FhirResponse.Ok(observation);
})
.RequireAuthorization("ClinicalRead")
.WithName("GetFhirObservation")
.WithSummary("Retrieve a rehabilitation pain observation");



app.MapGet("/fhir/Patient",
    (string? family, HttpRequest request) =>
    {
        var baseUrl =
            $"{request.Scheme}://{request.Host}";

        var bundle = FhirPatientSearch.Search(
            family,
            baseUrl);

        return FhirResponse.Ok(bundle);
    })
.RequireAuthorization("ClinicalRead")
.WithName("SearchFhirPatients")
.WithSummary("Search FHIR R4 patients by family name");


app.MapGet("/fhir/Observation",
    (string? patient, HttpRequest request) =>
    {
        var baseUrl =
            $"{request.Scheme}://{request.Host}";

        var bundle = FhirObservationSearch.Search(
            patient,
            baseUrl);

        return FhirResponse.Ok(bundle);
    })
.RequireAuthorization("ClinicalRead")
.WithName("SearchFhirObservations")
.WithSummary(
    "Search FHIR R4 observations by patient");



app.Run();

public partial class Program { }