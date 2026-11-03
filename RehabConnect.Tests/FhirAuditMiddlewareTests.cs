
using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace RehabConnect.Tests;

public class FhirAuditMiddlewareTests
{
    [Fact]
    public async Task UnauthorizedRequest_IsAudited()
    {
        using var factory = new AuditTestFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Remove("X-Test-User");
        client.DefaultRequestHeaders.Remove("X-Test-Role");

        var response = await client.GetAsync(
            "/fhir/Patient/patient-001");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        Assert.Contains(factory.Logs.Messages, message =>
            message.Contains("FHIR access audit:") &&
            message.Contains("UserId=anonymous") &&
            message.Contains("StatusCode=401"));
    }

    [Fact]
    public async Task ClinicianRequest_IsAuditedWithoutPatientDetails()
    {
        using var factory = new AuditTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/fhir/Patient/patient-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auditMessage = Assert.Single(
            factory.Logs.Messages.Where(message =>
                message.Contains("FHIR access audit:")));

        Assert.Contains("UserId=test-user", auditMessage);
        Assert.Contains("StatusCode=200", auditMessage);

        Assert.DoesNotContain("patient-001", auditMessage);
        Assert.DoesNotContain("John Smith", auditMessage);
        Assert.DoesNotContain("/fhir/Patient/", auditMessage);
        Assert.DoesNotContain("Bearer ", auditMessage);
    }

    private sealed class AuditTestFactory
        : RehabConnectWebApplicationFactory
    {
        public AuditLogCollector Logs { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddProvider(Logs);
            });
        }
    }

    private sealed class AuditLogCollector : ILoggerProvider
    {
        public ConcurrentBag<string> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) =>
            new CollectorLogger(Messages);

        public void Dispose() { }
    }

    private sealed class CollectorLogger : ILogger
    {
        private readonly ConcurrentBag<string> _messages;

        public CollectorLogger(ConcurrentBag<string> messages)
        {
            _messages = messages;
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                _messages.Add(formatter(state, exception));
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose() { }
    }
}
