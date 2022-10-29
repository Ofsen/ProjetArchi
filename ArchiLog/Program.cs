using ArchiLog.Data;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "NFKY - REST API",
        Description = "An ASP.NET Core Web API made by Entity Framework Core and a Library that simplifies models, controllers & context creation."
    });
    // Generate our own xml file of comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // Add our comments
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    // Add Library comments
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ArchiLibrary.xml"));
});

builder.Services.AddDbContext<ArchiLogDbContext>();

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

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
