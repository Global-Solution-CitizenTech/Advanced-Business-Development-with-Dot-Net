using ZelaFix.Api.Endpoints;
using ZelaFix.Api.Exceptions;
using ZelaFix.Api.Extensions;
using ZelaFix.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ZelaFixOracle")
                       ?? throw new InvalidOperationException(
                           "ConnectionStrings:ZelaFixOracle não configurada (use user-secrets ou variável de ambiente).");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddZelaFixApplicationServices();
builder.Services.AddZelaFixJwtAuthentication(builder.Configuration);
builder.Services.AddZelaFixRateLimiting(builder.Configuration);
builder.Services.AddZelaFixHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddZelaFixSwagger();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ZelaFix API v1");
    options.DocumentTitle = "ZelaFix API";
    options.RoutePrefix = string.Empty; // Swagger UI na raiz
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCategoriaEndpoints();
app.MapChamadoEndpoints();
app.MapEngajamentoEndpoints();
app.MapZelaFixHealthChecks();

app.Run();

public partial class Program;