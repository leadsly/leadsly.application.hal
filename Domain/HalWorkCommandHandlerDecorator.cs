using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Domain
{
    public class HalWorkCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand 
    {
        public HalWorkCommandHandlerDecorator(ICommandHandler<TCommand> decorated, ILogger<HalWorkCommandHandlerDecorator<TCommand>> logger)
        {
            _decorated = decorated;
            _logger = logger;
        }
        
        private readonly ICommandHandler<TCommand> _decorated;
        private readonly ILogger<HalWorkCommandHandlerDecorator<TCommand>> _logger;

        public async Task HandleAsync(TCommand command)
        {
            _logger.LogInformation("HalWorkCommandHandler executing");
            // check to see if right now is during hal's work day
            string timeZone = command.TimeZoneId;
            _logger.LogInformation("User's timezone is {timeZone}", timeZone);
       
            DateTime startOfWorkDay = DateTime.Parse(command.StartOfWorkDay);
            _logger.LogDebug($"Hal's start of work day is {startOfWorkDay.Date}");
            DateTime endOfWorkDay = DateTime.Parse(command.EndOfWorkDay);
            _logger.LogDebug($"Hal's end of work day is {endOfWorkDay.Date}");
            DateTime nowLocal = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, timeZone);
            _logger.LogDebug($"User's local now time is {nowLocal.Date}");

            if ((nowLocal > startOfWorkDay) && (nowLocal < endOfWorkDay))
            {
                await this._decorated.HandleAsync(command);
            }            
        }
    }
}
