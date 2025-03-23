using HBOICTKeuzewijzer.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace HBOICTKeuzewijzer.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class EnumAuthorizeAttribute : AuthorizeAttribute
    {
        public EnumAuthorizeAttribute(params Role[] roles)
        {
            Roles = string.Join(",", roles.Select(r => r.ToString()));
        }
    }
}
