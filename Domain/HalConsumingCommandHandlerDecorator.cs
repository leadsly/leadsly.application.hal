using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class HalConsumingCommandHandlerDecorator<TCommand> : IConsumeCommandHandler<TCommand>
        where TCommand : IConsumeCommand
    {
        public HalConsumingCommandHandlerDecorator(IConsumeCommandHandler<TCommand> decorated)
        {
            _decoratoed = decorated;
        }

        private readonly IConsumeCommandHandler<TCommand> _decoratoed;

        public async Task ConsumeAsync(TCommand command)
        {
            await this._decoratoed.ConsumeAsync(command);
        }
    }
}
