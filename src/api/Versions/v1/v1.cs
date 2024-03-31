using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace Homie.Api.v1
{
    sealed public class Version
    {
        private Version() { }

        public const uint Numeric = 1;
        public const string Name = "v1";
        public const string Title = "Homie Backoffice";
        public const bool Deprecated = false;
        public static OpenApiInfo ApiInfo => new OpenApiInfo() { 
            Title = Title,
            Description = Title + ", An ASP.NET Core Web API",
            Version = Name
        };
        public static ApiVersion ApiVersion => new ApiVersion(Numeric);
    }
}

