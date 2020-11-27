using API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace API
{
    public class ControllerModelConvention : IControllerModelConvention
    {
        private AuthorizationPolicy _policy;

        public ControllerModelConvention(AuthorizationPolicy policy)
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
