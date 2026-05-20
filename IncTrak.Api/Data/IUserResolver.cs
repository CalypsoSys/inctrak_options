using Microsoft.AspNetCore.Http;

namespace IncTrak.Data
{
    public interface IUserResolver
    {
        UserContext ResolveUser(HttpContext httpContext);
    }
}
