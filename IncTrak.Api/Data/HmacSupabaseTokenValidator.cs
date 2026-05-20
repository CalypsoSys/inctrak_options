using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace IncTrak.Data
{
    public class HmacSupabaseTokenValidator : ISupabaseTokenValidator
    {
        private readonly AppSettings _settings;

        public HmacSupabaseTokenValidator(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public Task<SupabaseIdentity> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_settings.GetSupabaseJwtSecret()))
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            string encodedHeader = parts[0];
            string encodedPayload = parts[1];
            string encodedSignature = parts[2];

            JsonElement header = ParseJson(encodedHeader);
            if (header.ValueKind == JsonValueKind.Undefined ||
                header.TryGetProperty("alg", out JsonElement algorithm) == false ||
                string.Equals(algorithm.GetString(), "HS256", StringComparison.Ordinal) == false)
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            byte[] expectedSignature = ComputeSignature($"{encodedHeader}.{encodedPayload}", _settings.GetSupabaseJwtSecret());
            byte[] providedSignature = DecodeBase64Url(encodedSignature);
            if (providedSignature == null || CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature) == false)
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            JsonElement payload = ParseJson(encodedPayload);
            if (payload.ValueKind == JsonValueKind.Undefined)
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            if (payload.TryGetProperty("exp", out JsonElement expElement) &&
                expElement.ValueKind == JsonValueKind.Number &&
                expElement.TryGetInt64(out long expSeconds) &&
                DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(expSeconds))
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            if (payload.TryGetProperty("sub", out JsonElement subElement) == false)
            {
                return Task.FromResult(new SupabaseIdentity());
            }

            return Task.FromResult(new SupabaseIdentity
            {
                ExternalIdentity = subElement.GetString(),
                EmailAddress = payload.TryGetProperty("email", out JsonElement emailElement) ? emailElement.GetString() : null
            });
        }

        private static JsonElement ParseJson(string encodedJson)
        {
            byte[] bytes = DecodeBase64Url(encodedJson);
            if (bytes == null)
            {
                return default;
            }

            using JsonDocument document = JsonDocument.Parse(bytes);
            return document.RootElement.Clone();
        }

        private static byte[] ComputeSignature(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
        }

        private static byte[] DecodeBase64Url(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string normalized = value.Replace('-', '+').Replace('_', '/');
            while (normalized.Length % 4 != 0)
            {
                normalized += "=";
            }

            try
            {
                return Convert.FromBase64String(normalized);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
