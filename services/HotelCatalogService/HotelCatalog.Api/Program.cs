using Booking.Shared.Api;
using Booking.Shared.Identity;
using Booking.Shared.Infrastructure.Messaging;
using HotelCatalog.Application;
using HotelCatalog.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<HotelCatalogDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("HotelCatalogDatabase"));
});

builder.Services.AddRabbitMq();

builder.Services.AddApplication();
builder.AddKeycloakAuthentication(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<HotelCatalogDbContext>();
await context.Database.MigrateAsync();

app.UseExceptionHandler();

app.MapControllers();

app.Run();