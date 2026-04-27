using System.Threading.Tasks;

namespace IncTrak.Data
{
    public interface ISupabaseTokenValidator
    {
        Task<SupabaseIdentity> ValidateTokenAsync(string token);
    }
}
