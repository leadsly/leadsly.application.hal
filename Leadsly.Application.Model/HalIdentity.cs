using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    public class HalIdentity : IHalIdentity
    {
        public HalIdentity(string id)
        {
            Id = id;
        }
        public string Id { get; }
    }
}
