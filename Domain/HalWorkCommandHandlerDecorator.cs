using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Domain
{
    public class HalWorkCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand 
    {
        public HalWorkCommandHandlerDecorator(ICommandHandler<TCommand> decorated, ILogger<HalWorkCommandHandlerDecorator<TCommand>> logger, ITimestampService timestampService)
        {
            _decorated = decorated;
            _logger = logger;
            _timestampService = timestampService;
        }
        
        private readonly ITimestampService _timestampService;
        private readonly ICommandHandler<TCommand> _decorated;
        private readonly ILogger<HalWorkCommandHandlerDecorator<TCommand>> _logger;

        public async Task HandleAsync(TCommand command)
        {
            _timestampService.GetDateTimeOffsetLocal(command.TimeZoneId, 1654053761);
            _logger.LogInformation("HalWorkCommandHandler executing");
            // check to see if right now is during hal's work day
            string tzId = command.TimeZoneId;
            _logger.LogInformation("User's TimeZone id is {tzId}", tzId);            

            _logger.LogDebug($"Start of work day value is {command.StartOfWorkDay}");
            DateTimeOffset startOfWorkDay = _timestampService.ParseDateTimeOffsetLocalized(tzId, command.StartOfWorkDay);
            _logger.LogDebug($"Hal's start of work day is {startOfWorkDay}");

            _logger.LogDebug($"End of work day value is {command.EndOfWorkDay}");
            DateTimeOffset endOfWorkDay = _timestampService.ParseDateTimeOffsetLocalized(tzId, command.EndOfWorkDay);
            _logger.LogDebug($"Hal's end of work day is {endOfWorkDay}");

            DateTimeOffset nowLocal = _timestampService.GetNowLocalized(tzId);
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
