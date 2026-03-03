using DeviceDataBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Allow CORS from the Blazor frontend dev URL so the browser can call this API during development.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7273", "http://localhost:5134")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.WebHost.UseUrls("https://localhost:7027");

builder.Services.AddControllers();
// Register in-memory device data store
builder.Services.AddSingleton<IDeviceDataStore, DeviceDataStore>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();



// Enable CORS for requests from the frontend dev server
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
