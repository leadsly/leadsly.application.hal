using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IConsumeCommandHandler<TCommand> where TCommand : IConsumeCommand
    {
        Task ConsumeAsync(TCommand command);
    }
}
