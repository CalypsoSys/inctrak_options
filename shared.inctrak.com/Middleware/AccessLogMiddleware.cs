using IncTrak.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IncTrak.Middleware
{
    public class AccessLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _settings;

        public AccessLogMiddleware(RequestDelegate next, IOptions<AppSettings> options)
        {
            _next = next;
            _settings = options.Value ?? new AppSettings();
        }

        public async Task Invoke(HttpContext context)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                string remoteIp = FileLogWriter.SanitizeSingleLine(GetRemoteIp(context));
                string method = FileLogWriter.SanitizeSingleLine(context.Request.Method);
                string path = FileLogWriter.SanitizeSingleLine(BuildLogPath(context.Request));
                string protocol = FileLogWriter.SanitizeSingleLine(context.Request.Protocol);
                string referer = FileLogWriter.SanitizeSingleLine(context.Request.Headers.Referer.ToString());
                string userAgent = FileLogWriter.SanitizeSingleLine(context.Request.Headers.UserAgent.ToString());
                string contentLength = context.Response.ContentLength.HasValue ? context.Response.ContentLength.Value.ToString() : "-";

                string line = string.Format(
                    "{0} - - [{1:dd/MMM/yyyy:HH:mm:ss zzz}] \"{2} {3} {4}\" {5} {6} \"{7}\" \"{8}\" {9}ms",
                    remoteIp,
                    DateTimeOffset.Now,
                    method,
                    path,
                    protocol,
                    context.Response.StatusCode,
                    contentLength,
                    referer,
                    userAgent,
                    stopwatch.ElapsedMilliseconds);

                FileLogWriter.WriteLine(_settings.GetAccessLogPath(), line);
            }
        }

        private static string GetRemoteIp(HttpContext context)
        {
            string cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].ToString();
            if (string.IsNullOrWhiteSpace(cfConnectingIp) == false)
                return cfConnectingIp;

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        public static string BuildLogPath(HttpRequest request)
        {
            string path = request.Path.ToString();
            if (request.QueryString.HasValue == false)
                return path;

            var query = QueryHelpers.ParseQuery(request.QueryString.Value);
            if (query.Count == 0)
                return path;

            IEnumerable<string> pairs = query.Select(entry =>
            {
                string value = IsSensitiveQueryKey(entry.Key)
                    ? "[REDACTED]"
                    : string.Join(",", entry.Value);
                return string.Format("{0}={1}", entry.Key, value);
            });

            return string.Format("{0}?{1}", path, string.Join("&", pairs));
        }

        private static bool IsSensitiveQueryKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            string normalized = key.Trim().ToLowerInvariant();
            return normalized.Contains("token") ||
                   normalized.Contains("secret") ||
                   normalized.Contains("password") ||
                   normalized.Contains("code") ||
                   normalized.Contains("uuid") ||
                   normalized.EndsWith("key");
        }
    }
}
