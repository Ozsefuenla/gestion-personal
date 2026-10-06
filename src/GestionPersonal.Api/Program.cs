using GestionPersonal.Api.Endpoints;
using GestionPersonal.Api.Middleware;
using GestionPersonal.Application;
using GestionPersonal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { name = "GestionPersonal API", status = "ok" }));

app.MapWorkersEndpoints();

app.Run();

public partial class Program { }
