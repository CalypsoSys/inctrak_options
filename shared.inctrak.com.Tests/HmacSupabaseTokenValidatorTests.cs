using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IncTrak.Data;
using Microsoft.Extensions.Options;
using Xunit;

namespace inctrak.com.Tests
{
    public class HmacSupabaseTokenValidatorTests
    {
        [Fact]
        public async Task ValidateToken_ReturnsIdentityForValidToken()
        {
            var validator = new HmacSupabaseTokenValidator(Options.Create(new AppSettings
            {
                SupabaseJwtSecret = "super-secret"
            }));
            string token = CreateToken("super-secret", "33333333-3333-3333-3333-333333333333", "founder@calypsosys.com", DateTimeOffset.UtcNow.AddMinutes(10));

            SupabaseIdentity identity = await validator.ValidateTokenAsync(token);

            Assert.Equal("33333333-3333-3333-3333-333333333333", identity.ExternalIdentity);
            Assert.Equal("founder@calypsosys.com", identity.EmailAddress);
        }

        [Fact]
        public async Task ValidateToken_RejectsExpiredToken()
        {
            var validator = new HmacSupabaseTokenValidator(Options.Create(new AppSettings
            {
                SupabaseJwtSecret = "super-secret"
            }));
            string token = CreateToken("super-secret", "33333333-3333-3333-3333-333333333333", "founder@calypsosys.com", DateTimeOffset.UtcNow.AddMinutes(-10));

            SupabaseIdentity identity = await validator.ValidateTokenAsync(token);

            Assert.False(identity.IsAuthenticated());
        }

        private static string CreateToken(string secret, string subject, string email, DateTimeOffset expires)
        {
            string header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
            {
                alg = "HS256",
                typ = "JWT"
            }));
            string payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
            {
                sub = subject,
                email,
                exp = expires.ToUnixTimeSeconds()
            }));
            string message = $"{header}.{payload}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            string signature = Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(message)));
            return $"{message}.{signature}";
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
