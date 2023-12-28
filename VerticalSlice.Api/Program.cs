using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Api.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database Connection String
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

// assembly 
var assembly = typeof(Program).Assembly;

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

// Carter
builder.Services.AddCarter();

// FluentValidation
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Carter
app.MapCarter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
