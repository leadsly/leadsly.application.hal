using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interactions.Networking.NoResultsFound.Interfaces
{
    public interface INoResultsFoundInteractionHandler<TInteraction> : IInteractionHandler<TInteraction> 
        where TInteraction : IInteraction
    {
    }
}
