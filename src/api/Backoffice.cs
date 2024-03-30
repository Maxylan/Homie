using System.Reflection;
using Homie.Api.v1;

namespace Homie 
{
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

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(
                options => {
                    options.SwaggerDoc("v1", v1.ApiInfo);

                    options.IncludeXmlComments(
                        Path.Combine(
                            AppContext.BaseDirectory,$"{Assembly.GetExecutingAssembly().GetName().Name}.xml"
                        ), true
                    );
                }
            );

            // For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.

            App = builder.Build();
            isDevelopment = App.Environment.IsDevelopment();

            // Configure the HTTP request pipeline.
            if (isDevelopment)
            {
                App.UseSwagger();
                App.UseSwaggerUI(
                    options => {
                        foreach (var description in App.DescribeApiVersions())
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

            App.UseHttpLogging();
            App.UseAuthorization();
            App.MapControllers();

            App.Run();
        }
    }
}

