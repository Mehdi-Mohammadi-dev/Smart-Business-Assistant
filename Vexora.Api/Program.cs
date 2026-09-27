using Microsoft.Extensions.Caching.Distributed;
using Vexora.Application.DependencyInjections;
using Vexora.Infrastracture.Registrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastracture(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

using var scope = app.Services.CreateScope();

var cache =
    scope.ServiceProvider
        .GetRequiredService<IDistributedCache>();

await cache.SetStringAsync(
    "test",
    "hello");

var value =
    await cache.GetStringAsync("test");

Console.WriteLine(value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
