using Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class HalWorkCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand 
    {
        public HalWorkCommandHandlerDecorator(ICommandHandler<TCommand> decorated)
        {
            _decorated = decorated;
        }
        
        private readonly ICommandHandler<TCommand> _decorated;

        public async Task HandleAsync(TCommand command)
        {
            // check to see if right now is during hal's work day
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(command.TimeZoneId);                        
            DateTime startOfWorkDay = DateTime.Parse(command.StartOfWorkDay);
            DateTime endOfWorkDay = DateTime.Parse(command.EndOfWorkDay);

            DateTime usersTime = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

            if ((usersTime > startOfWorkDay) && (usersTime < endOfWorkDay))
            {
                await this._decorated.HandleAsync(command);
            }            
        }
    }
}
