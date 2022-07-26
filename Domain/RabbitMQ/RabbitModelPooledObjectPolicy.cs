﻿using Domain.OptionsJsonModels;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Domain.RabbitMQ
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
            var factory = new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryConfigOptions.UserName,
                Password = options.ConnectionFactoryConfigOptions.Password,
                HostName = options.ConnectionFactoryConfigOptions.HostName,
                Port = options.ConnectionFactoryConfigOptions.Port,
                VirtualHost = options.ConnectionFactoryConfigOptions.VirtualHost,
                DispatchConsumersAsync = true,
                ClientProvidedName = $"[Consumer] HalId: {halId}",
                Ssl = new SslOption()
                {
                    Enabled = options.ConnectionFactoryConfigOptions.Ssl.Enabled
                }
            };

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
