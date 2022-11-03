using ArchiLog.Data;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Configuration;
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

logger.Information("LOG : Initialisation");

// Versioning
builder.Services.AddApiVersioning(option =>
{
    option.AssumeDefaultVersionWhenUnspecified = true;
    // if not specified, default version is 1.0
    option.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    option.ReportApiVersions = true;

    // We can add these to accept headers, query strings and media type for our versioning control
    //option.ApiVersionReader = ApiVersionReader.Combine(
    //    new QueryStringApiVersionReader("api-version"),
    //    new HeaderApiVersionReader("X-Version"),
    //    new MediaTypeApiVersionReader("ver"));
});

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
