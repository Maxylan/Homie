// (c) 2024 @Maxylan
namespace Homie.Api.v1;

using Asp.Versioning;
using Microsoft.OpenApi.Models;

sealed public class Version
{
    private Version() { }

    public const uint Numeric = 1;
    public const string Name = "v1";
    public const string Title = "Homie Backoffice";
    public const bool Deprecated = false;
    public static OpenApiInfo ApiInfo => new OpenApiInfo() { 
        Title = Title + $" {Backoffice.homieVersion} ({Backoffice.homieApiVersion}, \"{Name}\")",
        Description = Title + ", an ASP.NET Core 8.0 (C# 12) Web API made for our all-purpose home-convenience app: Homie!",
        // Version = $"<span id=\"homie-version\">{Backoffice.homieVersion}</span> {Backoffice.homieApiVersion} {Name}",
        Version = Name,
        Contact = new OpenApiContact() { 
            Name = "Maxylan", 
            Email = "max100@live.se", 
            Url = new System.Uri("https://github.com/Maxylan") 
        },
        License = new OpenApiLicense() { 
            Name = "MIT", 
            Url = new System.Uri("https://opensource.org/licenses/MIT") 
        },
    };
    public static ApiVersion ApiVersion => new ApiVersion(Numeric);
}