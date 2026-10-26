using Hl7.Fhir.Serialization;
using RehabConnect.Api.Fhir;

var builder = WebApplication.CreateBuilder(args);

// Register OpenAPI documentation services
builder.Services.AddOpenApi();

var app = builder.Build();

// OpenAPI is available in development only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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
        var error = new
        {
            resourceType = "OperationOutcome",
            issue = new[]
            {
                new
                {
                    severity = "error",
                    code = "not-found",
                    diagnostics = "Patient not found"
                }
            }
        };

        return Results.Json(
            error,
            statusCode: 404,
            contentType: "application/fhir+json"
        );
    }

    var patient = FhirSampleData.CreatePatient();

    var serializer = new FhirJsonSerializer();
    var json = serializer.SerializeToString(patient);

    return Results.Json(
        json,
        contentType: "application/fhir+json"
    );
})
.WithName("GetFhirPatient")
.WithSummary("Retrieve a synthetic FHIR patient");


app.MapGet("/fhir/Observation/{id}", (string id) =>
{
    if (id != "pain-001")
    {
        return Results.NotFound();
    }

    var observation = FhirSampleData.CreatePainObservation();

    var serializer = new FhirJsonSerializer();
    var json = serializer.SerializeToString(observation);

    return Results.Content(
        json,
        "application/fhir+json");
})
.WithName("GetFhirObservation")
.WithSummary("Retrieve a rehabilitation pain observation");


app.Run();
