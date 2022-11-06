using ArchiLog.Data;
using ArchiLog.Options;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    //options.SwaggerDoc("v1", new OpenApiInfo
    //{
    //    Version = "1",
    //    Title = "NFKY - REST API",
    //    Description = "An ASP.NET Core Web API made by Entity Framework Core and a Library that simplifies models, controllers & context creation."
    //});
    //options.SwaggerDoc("v2", new OpenApiInfo
    //{
    //    Version = "2",
    //    Title = "NFKY - REST API v2",
    //    Description = "An ASP.NET Core Web API made by Entity Framework Core and a Library that simplifies models, controllers & context creation."
    //});

    // Generate our own xml file of comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // Add our comments
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    // Add Library comments
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ArchiLibrary.xml"));
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddDbContext<ArchiLogDbContext>();

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

logger.Information("LOG : Initialisation");

// Versioning
builder.Services.AddApiVersioning(option =>
{
    // if not specified, default version is 1.0
    option.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.ReportApiVersions = true;
    option.ApiVersionReader = new UrlSegmentApiVersionReader();

    // We can add these to accept headers, query strings and media type for our versioning control
    //option.ApiVersionReader = ApiVersionReader.Combine(
    //    new QueryStringApiVersionReader("api-version"),
    //    new HeaderApiVersionReader("X-Version"),
    //    new MediaTypeApiVersionReader("ver"));
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();


// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach(var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.ApiVersion.ToString()
                );
        }
    });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
