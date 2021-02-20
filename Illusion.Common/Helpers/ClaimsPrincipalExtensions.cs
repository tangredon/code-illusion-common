using System;
using System.Security.Claims;

namespace Illusion.Common.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetGuid(this ClaimsPrincipal principal)
        {
            if (!(principal.Identity is ClaimsIdentity identity))
            {
                throw new Exception("Invalid user identity");
            }

            var name = identity.Name;
            if (Guid.TryParse(name, out var guid))
            {
                return guid;
            }
            else
            {
                throw new Exception("Invalid user identifier");
            }
        }
    }
}
