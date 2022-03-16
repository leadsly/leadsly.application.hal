using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public interface IFileManager
    {
        public HalOperationResult<T> CloneDefaultChromeProfile<T>(string newChromeProfileName, WebDriverOptions options)
            where T : IOperationResponse;

        public HalOperationResult<T> RemoveDirectory<T>(string directory)
            where T : IOperationResponse;
        public List<string> LoadExistingChromeProfiles(string browserPurpose, WebDriverOptions options);

    }
}
