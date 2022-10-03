﻿using Domain.OptionsJsonModels;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Domain.MQ
{
    public class RabbitModelPooledObjectPolicy : PooledObjectPolicy<IModel>
    {
        public RabbitModelPooledObjectPolicy(RabbitMQConfigOptions options, string halId)
        {
            _connection = GetConnection(options, halId);
        }

        private readonly IConnection _connection;

        private IConnection GetConnection(RabbitMQConfigOptions options, string halId)
        {
            var factory = new ConnectionFactory();

            if (options.ConnectionFactoryConfigOptions.Ssl.Enabled == true)
            {
                factory.Ssl = new SslOption
                {
                    Enabled = options.ConnectionFactoryConfigOptions.Ssl.Enabled,
                    ServerName = options.ConnectionFactoryConfigOptions.Ssl.ServerName
                };
            }

            factory.HostName = options.ConnectionFactoryConfigOptions.HostName;
            factory.UserName = options.ConnectionFactoryConfigOptions.UserName;
            factory.Password = options.ConnectionFactoryConfigOptions.Password;
            factory.Port = options.ConnectionFactoryConfigOptions.Port;
            factory.DispatchConsumersAsync = true;
            factory.ClientProvidedName = $"[Consumer] HalId: {halId}";

            return factory.CreateConnection();
        }

        public override IModel Create()
        {
            return _connection.CreateModel();
        }

        public override bool Return(IModel obj)
        {
            if (obj.IsOpen)
            {
                return true;
            }
            else
            {
                obj?.Dispose();
                return false;
            }
        }
    }
}