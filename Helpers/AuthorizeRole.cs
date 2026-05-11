using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TANA.Helpers;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true
)]
public class AuthorizeRole : AuthorizeAttribute, IAuthorizationFilter
{
    public AuthorizeRole() { }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity.IsAuthenticated)
        {
            // it isn't needed to set unauthorized result
            // as the base class already requires the user to be authenticated
            return;
        }
        Rbac rbac = context.HttpContext.RequestServices.GetService<Rbac>();
        string page = context.RouteData.Values["controller"].ToString();
        string action = context.RouteData.Values["action"].ToString();
        var roleClaim = user.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleClaim))
        {
            context.Result = new ForbidResult();
            return;
        }
        var userRole = int.Parse(roleClaim);

        bool allow = rbac.GetPageAccess(userRole, page, action);
        if (!allow)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
