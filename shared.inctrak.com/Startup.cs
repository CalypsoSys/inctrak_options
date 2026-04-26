using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using IncTrak.Data;

namespace inctrak.com
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            AppSettings appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;
                options.RequireHeaderSymmetry = false;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("FrontendOrigins", policy =>
                {
                    string[] allowedOrigins = appSettings.AllowedOrigins ?? Array.Empty<string>();
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
                });
            });
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<AppSettings> appSettingsOptions)
        {
            AppSettings appSettings = appSettingsOptions.Value;

            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (env.IsDevelopment() ||
                    appSettings.RequireGatewaySecret == false ||
                    string.IsNullOrWhiteSpace(appSettings.GatewaySecret))
                {
                    await next();
                    return;
                }

                string headerName = string.IsNullOrWhiteSpace(appSettings.GatewaySecretHeaderName)
                    ? "X-Internal-Api-Key"
                    : appSettings.GatewaySecretHeaderName;

                if (context.Request.Headers.TryGetValue(headerName, out var providedSecret) == false ||
                    string.Equals(providedSecret, appSettings.GatewaySecret, StringComparison.Ordinal) == false)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                await next();
            });
            app.UseCors("FrontendOrigins");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
            });
        }
    }
}
