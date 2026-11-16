using Serilog;
using ZelaFix.Api.Endpoints;
using ZelaFix.Api.Exceptions;
using ZelaFix.Api.Extensions;
using ZelaFix.Api.Middleware;
using ZelaFix.Infrastructure;
using ZelaFix.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Compatibilidade com PaaS (Render, Cloud Run, etc.) que definem a porta via variável PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ZelaFix.Api")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/zelafix-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) {SourceContext}: {Message:lj}{NewLine}{Exception}"));

var connectionString = builder.Configuration.GetConnectionString("ZelaFixOracle")
                       ?? throw new InvalidOperationException(
                           "ConnectionStrings:ZelaFixOracle não configurada (use user-secrets ou variável de ambiente).");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddZelaFixApplicationServices();
builder.Services.AddZelaFixJwtAuthentication(builder.Configuration);
builder.Services.AddZelaFixRateLimiting(builder.Configuration);
builder.Services.AddZelaFixHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddZelaFixSwagger();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services, app.Logger);

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ZelaFix API v1");
    options.DocumentTitle = "ZelaFix API";
    options.RoutePrefix = string.Empty; // Swagger UI na raiz
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableFilter();
});

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCategoriaEndpoints();
app.MapChamadoEndpoints();
app.MapZelaFixHealthChecks();

app.Run();

public partial class Program;
