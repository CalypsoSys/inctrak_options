using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using IncTrak.Data;
using IncTrak.Middleware;
using System.Threading.RateLimiting;

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
            services.AddHttpClient(nameof(SupabaseTokenValidator));
            services.AddSingleton<RequestContextAccessor>();
            services.AddSingleton<HeaderTenantResolver>();
            services.AddSingleton<HeaderUserResolver>();
            services.AddSingleton<HeaderMembershipResolver>();
            services.TryAddSingleton<IControlPlaneStore, NpgsqlControlPlaneStore>();
            services.TryAddSingleton<HmacSupabaseTokenValidator>();
            services.TryAddSingleton<ISupabaseTokenValidator, SupabaseTokenValidator>();
            services.AddSingleton<ITenantResolver, ControlPlaneTenantResolver>();
            services.AddSingleton<IUserResolver, ControlPlaneUserResolver>();
            services.AddSingleton<IMembershipResolver, ControlPlaneMembershipResolver>();

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
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.Headers["Retry-After"] = Math.Max(1, appSettings.RateLimit.WindowSeconds).ToString();
                    await context.HttpContext.Response.WriteAsync("Too many requests.", token);
                };
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    if (appSettings.RateLimit.Enabled == false)
                    {
                        return RateLimitPartition.GetNoLimiter("disabled");
                    }

                    string partitionKey = httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? httpContext.Request.Headers["CF-Connecting-IP"].ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, appSettings.RateLimit.PermitLimit),
                        Window = TimeSpan.FromSeconds(Math.Max(1, appSettings.RateLimit.WindowSeconds)),
                        QueueLimit = Math.Max(0, appSettings.RateLimit.QueueLimit),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    });
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

            app.UseMiddleware<AccessLogMiddleware>();
            app.UseRouting();
            app.UseMiddleware<SupabaseAuthMiddleware>();
            app.UseMiddleware<ControlPlaneContextMiddleware>();
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
            app.UseRateLimiter();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
            });
        }
    }
}
