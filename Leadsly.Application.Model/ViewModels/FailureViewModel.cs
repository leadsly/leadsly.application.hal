using Leadsly.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels
{
    public class FailureViewModel
    {
        public Codes? Code { get; set; } = Codes.NONE;
        public string Detail { get; set; }
        public string Reason { get; set; }
    }
}
