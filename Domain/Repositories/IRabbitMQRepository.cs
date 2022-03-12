using Domain.OptionsJsonModels;
using Leadsly.Application.Model.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IRabbitMQRepository
    {
        RabbitMQOptions GetRabbitMQConfigOptions();
    }
}
