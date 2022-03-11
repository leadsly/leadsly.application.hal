using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IConsumingService
    {
        /// <summary>
        /// Start consuming (getting messages).
        /// </summary>
        void StartConsuming();
    }
}
