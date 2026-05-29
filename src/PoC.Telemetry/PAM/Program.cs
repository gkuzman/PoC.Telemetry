using Microsoft.Extensions.DependencyInjection.Extensions;
using PAM;
using PAM.DB;
using PAM.Services;
using Shared;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetry(TracingExtensions.Source.Name);
builder.AddSqlServerDbContext<PamDbContext>("pam-db");
builder.AddAzureServiceBusClient("servicebus");
builder.Services.AddServiceBusSenderService();
builder.Services.TryAddScoped<IWithdrawalService, WithdrawalService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PamDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();