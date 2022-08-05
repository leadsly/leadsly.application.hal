using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels
{
    public class AuthResponseViewModel
    {
        public ApplicationAccessTokenViewModel AccessToken { get; set; }
        public bool Is2StepVerificationRequired { get; set; }
        public string Provider { get; set; }
    }
}
