namespace IncTrak.Data
{
    public class RateLimitSettings
    {
        public bool Enabled { get; set; }
        public int PermitLimit { get; set; } = 120;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; }
    }

    public class AppSettings
    {
        public string[] AllowedOrigins { get; set; } = inctrak.com.CorsOriginPolicy.DefaultAllowedOrigins;
        public bool RequireGatewaySecret { get; set; }
        public string GatewaySecretHeaderName { get; set; } = "X-Internal-Api-Key";
        public string GatewaySecret { get; set; }
        public RateLimitSettings RateLimit { get; set; } = new RateLimitSettings();
        public string IncTrakDns { get; set; }
        public string GoogleSecretKey { get; set; }
        public string GoogleClientId { get; set; }
        public string IncTrakConnection { get; set; }
        public string ControlPlaneConnection { get; set; }
        public string SupabaseUrl { get; set; }
        public string SupabaseAnonKey { get; set; }
        public string SupabaseJwtSecret { get; set; }
        public string FeedbackConnection { get; set; }
        public string SlackFeedbackWebhookUrl { get; set; }
        public string AccessLogPath { get; set; } = "logs/access.log";
        public string ErrorLogPath { get; set; } = "logs/errors.log";
        public string TenantTemplateDatabaseName { get; set; } = "inctrak_template";
        public string TenantDatabasePrefix { get; set; } = "inctrak_";

        public string GetIncTrakDns()
        {
            return IncTrakDns;
        }

        public string GetGoogleSecretKey()
        {
            return GoogleSecretKey;
        }

        public string GetGoogleClientId()
        {
            return GoogleClientId;
        }

        public string GetIncTrakConnection()
        {
            return IncTrakConnection;
        }

        public string GetFeedbackConnection()
        {
            return FeedbackConnection;
        }

        public string GetControlPlaneConnection()
        {
            return ControlPlaneConnection;
        }

        public string GetSupabaseUrl()
        {
            return SupabaseUrl;
        }

        public string GetSupabaseAnonKey()
        {
            return SupabaseAnonKey;
        }

        public string GetSupabaseJwtSecret()
        {
            return SupabaseJwtSecret;
        }

        public string GetSlackFeedbackWebhookUrl()
        {
            return SlackFeedbackWebhookUrl;
        }

        public string GetAccessLogPath()
        {
            return AccessLogPath;
        }

        public string GetErrorLogPath()
        {
            return ErrorLogPath;
        }

        public string GetTenantTemplateDatabaseName()
        {
            return TenantTemplateDatabaseName;
        }

        public string GetTenantDatabasePrefix()
        {
            return TenantDatabasePrefix;
        }
    }
}
