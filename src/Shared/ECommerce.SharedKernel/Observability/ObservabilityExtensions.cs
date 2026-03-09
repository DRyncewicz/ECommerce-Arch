using ECommerce.SharedKernel.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace ECommerce.SharedKernel.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Adds Serilog (→ Seq) + OpenTelemetry tracing (→ Jaeger OTLP) + metrics (→ Prometheus).
    /// Call before builder.Build().
    /// </summary>
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder, string serviceName)
    {
        // ── Structured logging via Serilog → Seq ──────────────────────────────
        builder.Host.UseSerilog((ctx, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Seq(
                ctx.Configuration["Seq:ServerUrl"] ?? "http://seq:5341",
                apiKey: ctx.Configuration["Seq:ApiKey"]));

        // ── Traces → Jaeger (OTLP) + Metrics → Prometheus ────────────────────
        var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://jaeger:4317";

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName)
                .AddAttributes([new("deployment.environment",
                    builder.Environment.EnvironmentName)]))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        return builder;
    }

    /// <summary>
    /// Exposes /metrics endpoint for Prometheus scraping.
    /// Call after app.Build().
    /// </summary>
    public static WebApplication MapObservability(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapPrometheusScrapingEndpoint("/metrics");
        return app;
    }
}
