
namespace RehabConnect.Api.Audit;

public sealed class FhirAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FhirAuditMiddleware> _logger;

    public FhirAuditMiddleware(
        RequestDelegate next,
        ILogger<FhirAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/fhir"))
        {
            await _next(context);
            return;
        }

        await _next(context);

        // Use a pseudonymous identifier where possible.
        var userId = context.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? "anonymous";

        _logger.LogInformation(
            "FHIR access audit: UserId={UserId}, Method={Method}, " +
            "StatusCode={StatusCode}, TimestampUtc={TimestampUtc}",
            userId,
            context.Request.Method,
            context.Response.StatusCode,
            DateTimeOffset.UtcNow);
    }
}
