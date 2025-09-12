using Booking.Shared.Api;
using Booking.Shared.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Profile.Api.Consumers;
using Profile.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();

builder.Services.AddRabbitMq();

builder.Services.AddScoped<UserCreatedConsumer>();
builder.Services.AddHostedService<UserCreatedConsumerHostedService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddDbContext<ProfileDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProfileDatabase"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

await using var scope = app.Services.CreateAsyncScope();
var context = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
await context.Database.MigrateAsync();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.UseExceptionHandler();
app.MapControllers();

app.Run();