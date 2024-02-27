using System.Security.Claims;

namespace Core.DMS.Data.Brokers
{
    public class AuthenticationBroker(ClaimsPrincipal principal) : IAuthenticationBroker
    {
        public string GetUserId()
             => principal.Identity.Name; 
    }
}
