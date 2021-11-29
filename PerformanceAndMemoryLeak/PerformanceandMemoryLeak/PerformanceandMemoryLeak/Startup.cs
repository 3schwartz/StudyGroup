using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PerformanceandMemoryLeak
{
    public class Startup
    {
        public const string ActivitySourceName = "Test";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public static readonly ActivitySource ActivitySource = new ActivitySource(ActivitySourceName,
            typeof(Startup).Assembly.GetName().Version?.ToString());

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                var resourceBuilder = ResourceBuilder.CreateDefault();
                resourceBuilder.AddService("Test", null, null, false, Environment.MachineName);
                resourceBuilder.AddEnvironmentVariableDetector();

                builder.SetResourceBuilder(resourceBuilder);

                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();

                builder.AddSource(ActivitySourceName);

                services.Configure<JaegerExporterOptions>(Configuration.GetSection("Jaeger"));

                builder.AddJaegerExporter();
            });

            services.AddHostedService<SomeWork>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
