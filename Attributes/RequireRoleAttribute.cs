using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PharmaDNA.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RequireRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = context.HttpContext.Session.GetInt32("UserRole");
            var walletAddress = context.HttpContext.Session.GetString("WalletAddress");

            if (string.IsNullOrEmpty(walletAddress))
            {
                context.Result = new RedirectToPageResult("/Login");
                return;
            }

            // Role mapping: 0=None, 1=Manufacturer, 2=Distributor, 3=Pharmacy, 4=Admin
            var roleMap = new Dictionary<int, string>
            {
                { 1, "Manufacturer" },
                { 2, "Distributor" },
                { 3, "Pharmacy" },
                { 4, "Admin" }
            };

            if (userRole.HasValue && roleMap.ContainsKey(userRole.Value))
            {
                var userRoleName = roleMap[userRole.Value];
                if (!_allowedRoles.Contains(userRoleName))
                {
                    context.Result = new ForbidResult();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
