using Hal.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace Hal
{
    public class BearerTokenAuthorizeConvention : IControllerModelConvention
    {
        private AuthorizationPolicy _policy;

        public BearerTokenAuthorizeConvention(AuthorizationPolicy policy)
        {
            _policy = policy;
        }        

        public void Apply(ControllerModel controller)
        {
            if (controller.Filters.OfType<BearerTokenAuthorizeFilter>().FirstOrDefault() == null)
            {
                //default policy only used when there is no authorize filter in the controller
                controller.Filters.Add(new BearerTokenAuthorizeFilter(_policy));                
            }
        }
    }
}
