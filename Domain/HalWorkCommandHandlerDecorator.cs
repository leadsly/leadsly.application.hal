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
            string tzId = command.TimeZoneId;
            _logger.LogInformation("User's TimeZone id is {tzId}", tzId);
            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tzId);

            _logger.LogDebug($"Start of work day value is {command.StartOfWorkDay}");
            DateTimeOffset startOfWorkDay = DateTimeOffset.Parse(command.StartOfWorkDay);                        
            _logger.LogDebug($"Hal's start of work day is {startOfWorkDay}");

            _logger.LogDebug($"End of work day value is {command.EndOfWorkDay}");
            DateTimeOffset endOfWorkDay = DateTimeOffset.Parse(command.EndOfWorkDay);                        
            _logger.LogDebug($"Hal's end of work day is {endOfWorkDay}");

            DateTimeOffset nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tzInfo);
            _logger.LogDebug($"User's configured local now time is {nowLocal}");

            if ((nowLocal > startOfWorkDay) && (nowLocal < endOfWorkDay))
            {
                _logger.LogDebug("Task is within Hal's work day. Executing task...");
                await this._decorated.HandleAsync(command);
            }
            else
            {
                _logger.LogDebug("Task is outside of Hal's work day. The task will NOT be executed today");
            }            
        }
    }
}
