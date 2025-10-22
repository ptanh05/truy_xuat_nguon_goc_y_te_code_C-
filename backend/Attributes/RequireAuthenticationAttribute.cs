using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PharmaDNA.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAuthenticationAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var walletAddress = context.HttpContext.Session.GetString("WalletAddress");

            if (string.IsNullOrEmpty(walletAddress))
            {
                context.Result = new RedirectToPageResult("/Login");
            }
        }
    }
}
