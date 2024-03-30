using Microsoft.OpenApi.Models;

namespace Homie.Api.v1
{
    sealed public class v1
    {
        private v1() { }

        public const string Name = "v1";
        public const string Title = "Homie Backoffice";
        public const bool Deprecated = false;
        public static OpenApiInfo ApiInfo { get; } = new OpenApiInfo { 
            Title = Title,
            Description = Title + ", An ASP.NET Core Web API",
            Version = v1.Name
        };
    }
}

