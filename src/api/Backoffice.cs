// (c) 2024 @Maxylan
namespace Homie;

using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Homie.Database;
using Homie.Utilities.Converters;
using Microsoft.EntityFrameworkCore;
using v1 = Homie.Api.v1;

public class Backoffice
{
    #pragma warning disable CS8618
    /// <summary>
    /// The WebApplication instance.
    /// </summary>
    public static WebApplication App { get; private set; }
    /// <summary>
    /// A boolean value indicating whether the application is running in development mode.
    /// </summary>
    public static bool isDevelopment { get; private set; }
    /// <summary>
    /// A boolean value indicating whether the application is running in production.
    /// </summary>
    public static bool isProduction => !isDevelopment;
    #pragma warning disable CS8618

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Configuration.
        // builder.Services.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHomieServices();
        builder.Services
            .AddControllers()
            .AddJsonOptions(options => {
                options.AllowInputFormatterExceptionMessages = isDevelopment;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                // options.JsonSerializerOptions.Converters.Add(new MethodBaseJsonConverter());
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        
        // Learn more about versioning at https://www.milanjovanovic.tech/blog/api-versioning-in-aspnetcore
        builder.Services
            .AddApiVersioning(options => {
                options.DefaultApiVersion = v1.Version.ApiVersion;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                // options.ReportApiVersions = true;
            })
            .AddApiExplorer(options => {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddSwaggerGen(
            options => {
                options.SwaggerDoc("v1", v1.Version.ApiInfo);

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
            }
        );

        // For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        builder.Services.AddDbContext<HomieDB>(ServiceLifetime.Singleton);

        App = builder.Build();
        isDevelopment = App.Environment.IsDevelopment();

        // Configure the HTTP request pipeline.
        if (isDevelopment)
        {
            App.UseSwagger();
            App.UseSwaggerUI(
                options => {
                    var provider = App.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName
                        );
                    }
                }
            );
        }
        /* else 
        {
            App.UseHttpsRedirection();
            App.UseHsts();
        } */

        // App.UseHttpLogging();
        App.UseAuthorization();
        App.MapControllers();

        App.Run();
    }
}