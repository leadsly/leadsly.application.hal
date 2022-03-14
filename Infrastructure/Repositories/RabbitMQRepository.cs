using Domain.OptionsJsonModels;
using Domain.Repositories;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RabbitMQRepository : IRabbitMQRepository
    {
        public RabbitMQRepository(IOptions<RabbitMQConfigOptions> rabbitMQConfigOptions, ILogger<RabbitMQRepository> logger)
        {
            _logger = logger;
            _rabbitMQConfigOptions = rabbitMQConfigOptions.Value;
        }

        private readonly ILogger<RabbitMQRepository> _logger;
        private readonly RabbitMQConfigOptions _rabbitMQConfigOptions;

        public RabbitMQOptions GetRabbitMQConfigOptions()
        {
            return new RabbitMQOptions
            {
                RoutingKey = _rabbitMQConfigOptions.RoutingKey,
                ConnectionFactoryOptions = new()
                {
                    ClientProvidedName = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.ClientProvidedName,
                    HostName = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.HostName,
                    Password = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.Password,
                    Port = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.Port,
                    UserName = _rabbitMQConfigOptions.ConnectionFactoryConfigOptions.UserName,
                },
                ExchangeOptions = new()
                {
                    ExchangeType = _rabbitMQConfigOptions.ExchangeConfigOptions.ExchangeType,
                    Name = _rabbitMQConfigOptions.ExchangeConfigOptions.Name
                },
                QueueConfigOptions = new()
                {
                    Name = _rabbitMQConfigOptions.QueueConfigOptions.Name,
                    AutoAcknowledge = _rabbitMQConfigOptions.QueueConfigOptions.AutoAcknowledge
                }
            };
        }
    }
}
