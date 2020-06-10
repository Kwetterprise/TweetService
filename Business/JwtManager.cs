using System;
using System.Collections.Generic;

namespace Kwetterprise.TweetService.Business
{
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using Kwetterprise.EventSourcing.Client.Models.DataTransfer;

    public class JwtManager
    {
        private const string idClaimName = "Id";
        private const string roleClaimName = "Role";

        public (Guid Id, AccountRole Role) DeconstructClaims(IEnumerable<Claim> claims)
        {
            var claimList = claims.ToList();
            var id = JsonSerializer.Deserialize<Guid>(claimList.Single(x => x.Type == JwtManager.idClaimName).Value);
            var role = JsonSerializer.Deserialize<AccountRole>(claimList.Single(x => x.Type == JwtManager.roleClaimName).Value);

            return (id, role);
        }
    }
}
