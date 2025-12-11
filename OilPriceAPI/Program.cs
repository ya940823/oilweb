using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=OilPriceDB.db";
builder.Services.AddDbContext<OilPriceContext>(options =>
    options.UseSqlite(connectionString));

// Add HTTP Client
builder.Services.AddHttpClient();

// Add Services
builder.Services.AddScoped<OilPriceService>();
builder.Services.AddHostedService<OilPriceBackgroundService>();

// Add Static Files support
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();
    context.Database.EnsureCreated();
    
    // Seed initial data if database is empty
    if (!context.OilPrices.Any())
    {
        var random = new Random();
        var startDate = DateTime.Today.AddDays(-90);
        
        for (int i = 0; i <= 90; i++)
        {
            context.OilPrices.Add(new OilPriceAPI.Models.OilPrice
            {
                Date = startDate.AddDays(i),
                Gas92 = 29.8m + (decimal)(random.NextDouble() * 2 - 1),
                Gas95 = 31.3m + (decimal)(random.NextDouble() * 2 - 1),
                Gas98 = 33.3m + (decimal)(random.NextDouble() * 2 - 1),
                Diesel = 27.5m + (decimal)(random.NextDouble() * 2 - 1)
            });
        }
        
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles();

app.MapControllers();

// Serve SPA
app.MapFallbackToFile("index.html");

app.Run();
