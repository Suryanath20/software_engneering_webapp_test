using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ANDtr.Web.Models;

namespace ANDtr.Web.Common;

public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("FullName", string.IsNullOrWhiteSpace(user.FullName) ? (user.UserName ?? "User") : user.FullName));
        
        if (!string.IsNullOrEmpty(user.CompanyName))
        {
            identity.AddClaim(new Claim("CompanyName", user.CompanyName));
        }

        var roles = await UserManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "User";
        identity.AddClaim(new Claim("RoleLabel", primaryRole));
        identity.AddClaim(new Claim("MustChangePassword", user.MustChangePassword ? "true" : "false"));

        return identity;
    }
}
