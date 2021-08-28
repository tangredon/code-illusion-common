using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Illusion.Common.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Splitio.Services.Client.Interfaces;

namespace Illusion.Common.FeatureFlags
{
    internal class FeatureFlagProvider : IFeatureFlagProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISplitClient _client;
        private readonly ILogger<FeatureFlagProvider> _logger;

        public FeatureFlagProvider(IHttpContextAccessor httpContextAccessor, ISplitClient client, ILogger<FeatureFlagProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _client = client;
            _logger = logger;

            _client.BlockUntilReady(10000);
        }

        public bool GetFeatureFlag(string feature)
        {
            var key = BuildKey();
            var attributes = BuildAttributes();

            var treatment = _client.GetTreatment(key.ToString(), feature, attributes);

            switch (treatment)
            {
                case "on":
                    return true;
                case "off":
                    return false;
                default:
                    _logger.LogError($"Could not map feature={feature}; treatment={treatment}");
                    return false;
            }
        }

        private Guid BuildKey()
        {
            var principal = GetClaimsPrincipal();
            return principal.GetGuid();
        }

        private Dictionary<string, object> BuildAttributes()
        {
            var principal = GetClaimsPrincipal();
            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null) throw new InvalidOperationException("Invalid user");

            var claims = identity.Claims.ToList();

            var email = claims.First(c => c.Type == ClaimTypes.Email).Value;

            var dictionary = new Dictionary<string, object>()
            {
                { "email", email }
            };

            return dictionary;
        }

        private ClaimsPrincipal GetClaimsPrincipal()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) throw new InvalidOperationException("No http context found");

            var principal = context.User;
            if (principal == null) throw new InvalidOperationException("Invalid principal provided");

            return principal;
        }
    }
}
