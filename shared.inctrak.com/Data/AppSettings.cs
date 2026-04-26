namespace IncTrak.Data
{
    public class AppSettings
    {
        public string[] AllowedOrigins { get; set; } = inctrak.com.CorsOriginPolicy.DefaultAllowedOrigins;
        public bool RequireGatewaySecret { get; set; }
        public string GatewaySecretHeaderName { get; set; } = "X-Internal-Api-Key";
        public string GatewaySecret { get; set; }
        public string IncTrakDns { get; set; }
        public string IncTrakApiDns { get; set; }
        public string GoogleSecretKey { get; set; }
        public string GoogleClientId { get; set; }
        public string ErrorsHost { get; set; }
        public string ErrorsUsername { get; set; }
        public string ErrorsPassword { get; set; }
        public string IncTrakConnection { get; set; }
        public string FeedbackConnection { get; set; }
        public bool UseSNMP { get; set; }
        public string SNMPServer { get; set; }
        public int SNMPPort { get; set; }
        public string SNMPAddress { get; set; }
        public string SNMPPassword { get; set; }
        public string EmailApiKey { get; set; }
        public string EmailFrom { get; set; }
        public string ErrorLogPath { get; set; } = "logs/errors.log";

        public string GetIncTrakDns()
        {
            return IncTrakDns;
        }

        public string GetIncTrakApiDns()
        {
            return IncTrakApiDns;
        }

        public string GetGoogleSecretKey()
        {
            return GoogleSecretKey;
        }

        public string GetGoogleClientId()
        {
            return GoogleClientId;
        }

        public string GetErrorsHost()
        {
            return ErrorsHost;
        }

        public string GetErrorsUsername()
        {
            return ErrorsUsername;
        }

        public string GetErrorsPassword()
        {
            return ErrorsPassword;
        }

        public string GetIncTrakConnection()
        {
            return IncTrakConnection;
        }

        public string GetFeedbackConnection()
        {
            return FeedbackConnection;
        }

        public string GetSNMPServer()
        {
            return SNMPServer;
        }

        public string GetSNMPAddress()
        {
            return SNMPAddress;
        }

        public string GetSNMPPassword()
        {
            return SNMPPassword;
        }

        public string GetEmailApiKey()
        {
            return EmailApiKey;
        }

        public string GetEmailFrom()
        {
            return EmailFrom;
        }

        public string GetErrorLogPath()
        {
            return ErrorLogPath;
        }
    }
}
