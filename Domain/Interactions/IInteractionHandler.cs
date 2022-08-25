using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interactions
{
    public interface IInteractionHandler<TInteraction> where TInteraction : IInteraction
    {
        bool HandleInteraction(TInteraction interaction);
    }
}
