using Domain.Models;
using Leadsly.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IFileManager
    {
        public IOperationResult CloneDefaultChromeProfile(string profileDirectoryName, WebDriverOptions options);

    }
}
