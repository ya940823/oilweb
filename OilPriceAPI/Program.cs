using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString?.Contains("localdb") == true || connectionString?.Contains("Server") == true)
{
    builder.Services.AddDbContext<OilPriceContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Use SQLite as fallback for testing/development
    builder.Services.AddDbContext<OilPriceContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=oilprice.db"));
}

// Add HttpClient
builder.Services.AddHttpClient();

// Add custom services
builder.Services.AddScoped<OilPriceService>();

// Add background service
builder.Services.AddHostedService<OilPriceBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Ensure database is created and load initial data from XML files
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();
    context.Database.EnsureCreated();
    
    // On first run or when database is empty, load data from XML files into database
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    if (!context.OilPrices.Any())
    {
        logger.LogInformation("Database is empty. Loading initial data from XML files...");
        var oilPriceService = scope.ServiceProvider.GetRequiredService<OilPriceService>();
        var endDate = DateTime.Now;
        var startDate = endDate.AddDays(-365); // Load last year of data
        await oilPriceService.FetchAndSaveOilPricesAsync(startDate, endDate);
        logger.LogInformation("Initial data loaded from XML files into database.");
    }
}

app.Run();
