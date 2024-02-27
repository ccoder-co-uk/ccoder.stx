using System.Security.Principal;

namespace Core.DMS.Objects.DTOs
{
    public class CoreAuthInfo : IIdentity
    {
        public string AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }

        public string Name { get; set; }
    }
}
