using System.Text;
using TorrentScrapper.Api.Middleware;
using TorrentScrapper.Application.Abstractions;
using TorrentScrapper.Application.Services;
using TorrentScrapper.Infrastructure.Http;
using TorrentScrapper.Parsing.Rutor;
using TorrentScrapper.Parsing.Rutracker;

// Register code pages encoding provider for Windows-1251 (Rutracker)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ensure Unicode characters are not escaped in JSON
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            // Read allowed origins from configuration (appsettings.json). Fallback to localhost:4200.
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add HTTP client factory with scraper configuration
builder.Services.AddScraperHttpClients(builder.Configuration);

// Register application services
builder.Services.AddScoped<ITorrentService, TorrentService>();

// Register parsers
builder.Services.AddKeyedSingleton<ITorrentParser, RutorParser>("Rutor");
builder.Services.AddKeyedSingleton<ITorrentParser, RutrackerParser>("Rutracker");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use global error handling middleware
app.UseErrorHandling();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the Program class accessible to tests
public partial class Program { }
