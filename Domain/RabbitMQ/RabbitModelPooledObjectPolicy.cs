using Domain.OptionsJsonModels;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.RabbitMQ
{
    public class RabbitModelPooledObjectPolicy : PooledObjectPolicy<IModel>
    {
        public RabbitModelPooledObjectPolicy(RabbitMQConfigOptions options)
        {
            _connection = GetConnection(options);
        }

        private readonly IConnection _connection;

        private IConnection GetConnection(RabbitMQConfigOptions options)
        {
            var factory = new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryConfigOptions.UserName,
                Password = options.ConnectionFactoryConfigOptions.Password,
                HostName = options.ConnectionFactoryConfigOptions.HostName,
                Port = options.ConnectionFactoryConfigOptions.Port,
                VirtualHost = options.ConnectionFactoryConfigOptions.VirtualHost,
                DispatchConsumersAsync = true,
                ClientProvidedName = "[Consumer]"
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
