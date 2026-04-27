using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace IncTrak.Data
{
    public class SupabaseTokenValidator : ISupabaseTokenValidator
    {
        private readonly AppSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HmacSupabaseTokenValidator _hmacValidator;
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
        private JsonElement[] _cachedKeys = Array.Empty<JsonElement>();
        private DateTimeOffset _cacheExpiresAt = DateTimeOffset.MinValue;

        public SupabaseTokenValidator(
            IOptions<AppSettings> options,
            IHttpClientFactory httpClientFactory,
            HmacSupabaseTokenValidator hmacValidator)
        {
            _settings = options.Value;
            _httpClientFactory = httpClientFactory;
            _hmacValidator = hmacValidator;
        }

        public async Task<SupabaseIdentity> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new SupabaseIdentity();
            }

            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return new SupabaseIdentity();
            }

            JsonElement header = ParseJson(parts[0]);
            if (header.ValueKind == JsonValueKind.Undefined ||
                header.TryGetProperty("alg", out JsonElement algorithmElement) == false)
            {
                return new SupabaseIdentity();
            }

            string algorithm = algorithmElement.GetString();
            if (string.Equals(algorithm, "HS256", StringComparison.Ordinal))
            {
                return await _hmacValidator.ValidateTokenAsync(token);
            }

            if (string.IsNullOrWhiteSpace(_settings.GetSupabaseUrl()))
            {
                return new SupabaseIdentity();
            }

            JsonElement payload = ParseJson(parts[1]);
            if (payload.ValueKind == JsonValueKind.Undefined ||
                ValidatePayload(payload) == false)
            {
                return new SupabaseIdentity();
            }

            JsonElement jwk = await FindJwkAsync(header, algorithm);
            if (jwk.ValueKind == JsonValueKind.Undefined)
            {
                return new SupabaseIdentity();
            }

            byte[] data = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
            byte[] signature = DecodeBase64Url(parts[2]);
            if (signature == null || VerifySignature(jwk, algorithm, data, signature) == false)
            {
                return new SupabaseIdentity();
            }

            return new SupabaseIdentity
            {
                ExternalIdentity = payload.GetProperty("sub").GetString(),
                EmailAddress = payload.TryGetProperty("email", out JsonElement emailElement) ? emailElement.GetString() : null
            };
        }

        private async Task<JsonElement> FindJwkAsync(JsonElement header, string algorithm)
        {
            string keyId = header.TryGetProperty("kid", out JsonElement kidElement) ? kidElement.GetString() : null;
            JsonElement[] keys = await GetJwksKeysAsync();

            foreach (JsonElement key in keys)
            {
                string currentKid = key.TryGetProperty("kid", out JsonElement currentKidElement) ? currentKidElement.GetString() : null;
                string currentAlg = key.TryGetProperty("alg", out JsonElement currentAlgElement) ? currentAlgElement.GetString() : null;

                if (string.IsNullOrWhiteSpace(keyId) == false &&
                    string.Equals(keyId, currentKid, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(currentAlg) == false &&
                    string.Equals(algorithm, currentAlg, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                return key;
            }

            return default;
        }

        private async Task<JsonElement[]> GetJwksKeysAsync()
        {
            if (_cacheExpiresAt > DateTimeOffset.UtcNow && _cachedKeys.Length > 0)
            {
                return _cachedKeys;
            }

            await _cacheLock.WaitAsync();
            try
            {
                if (_cacheExpiresAt > DateTimeOffset.UtcNow && _cachedKeys.Length > 0)
                {
                    return _cachedKeys;
                }

                using HttpClient client = _httpClientFactory.CreateClient(nameof(SupabaseTokenValidator));
                using HttpResponseMessage response = await client.GetAsync(BuildJwksUrl());
                response.EnsureSuccessStatusCode();
                string body = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(body);

                if (document.RootElement.TryGetProperty("keys", out JsonElement keysElement) == false ||
                    keysElement.ValueKind != JsonValueKind.Array)
                {
                    _cachedKeys = Array.Empty<JsonElement>();
                    _cacheExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
                    return _cachedKeys;
                }

                var keys = new List<JsonElement>();
                foreach (JsonElement key in keysElement.EnumerateArray())
                {
                    keys.Add(key.Clone());
                }

                _cachedKeys = keys.ToArray();
                _cacheExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
                return _cachedKeys;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        private string BuildJwksUrl()
        {
            return $"{_settings.GetSupabaseUrl().TrimEnd('/')}/auth/v1/.well-known/jwks.json";
        }

        private bool ValidatePayload(JsonElement payload)
        {
            if (payload.TryGetProperty("sub", out JsonElement subElement) == false ||
                string.IsNullOrWhiteSpace(subElement.GetString()))
            {
                return false;
            }

            if (payload.TryGetProperty("exp", out JsonElement expElement) &&
                expElement.ValueKind == JsonValueKind.Number &&
                expElement.TryGetInt64(out long expSeconds) &&
                DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(expSeconds))
            {
                return false;
            }

            if (payload.TryGetProperty("iss", out JsonElement issuerElement))
            {
                string expectedIssuer = $"{_settings.GetSupabaseUrl().TrimEnd('/')}/auth/v1";
                if (string.Equals(issuerElement.GetString(), expectedIssuer, StringComparison.Ordinal) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool VerifySignature(JsonElement jwk, string algorithm, byte[] data, byte[] signature)
        {
            return algorithm switch
            {
                "ES256" => VerifyEcSignature(jwk, data, signature),
                "RS256" => VerifyRsaSignature(jwk, data, signature),
                _ => false
            };
        }

        private bool VerifyEcSignature(JsonElement jwk, byte[] data, byte[] signature)
        {
            if (jwk.TryGetProperty("crv", out JsonElement curveElement) == false ||
                string.Equals(curveElement.GetString(), "P-256", StringComparison.Ordinal) == false)
            {
                return false;
            }

            byte[] x = DecodeJsonWebKeyCoordinate(jwk, "x");
            byte[] y = DecodeJsonWebKeyCoordinate(jwk, "y");
            if (x == null || y == null)
            {
                return false;
            }

            using ECDsa ecdsa = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = x,
                    Y = y
                }
            });

            return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }

        private bool VerifyRsaSignature(JsonElement jwk, byte[] data, byte[] signature)
        {
            byte[] modulus = DecodeJsonWebKeyCoordinate(jwk, "n");
            byte[] exponent = DecodeJsonWebKeyCoordinate(jwk, "e");
            if (modulus == null || exponent == null)
            {
                return false;
            }

            using RSA rsa = RSA.Create(new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            });

            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        private static byte[] DecodeJsonWebKeyCoordinate(JsonElement jwk, string propertyName)
        {
            if (jwk.TryGetProperty(propertyName, out JsonElement valueElement) == false)
            {
                return null;
            }

            return DecodeBase64Url(valueElement.GetString());
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
