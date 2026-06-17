using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RoadIncidentApi.Data;
using RoadIncidentApi.Middleware;
using RoadIncidentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Kontrolery + serializacja enumów jako tekstu ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "System zgłoszeń zdarzeń drogowych",
        Version = "v1",
        Description = "API do zgłaszania i przeglądania zdarzeń drogowych (wypadki, roboty drogowe)."
    });
});

// --- Baza danych (EF Core + SQLite) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Dependency Injection: warstwa serwisów ---
builder.Services.AddScoped<IReporterService, ReporterService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

var app = builder.Build();

// --- Migracje + dane startowe przy starcie aplikacji ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// --- Globalna obsługa błędów (przed routingiem) ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
