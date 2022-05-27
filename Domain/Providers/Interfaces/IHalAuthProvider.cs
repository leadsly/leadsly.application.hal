using Domain.Models.Requests;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Interfaces
{
    public interface IHalAuthProvider
    {
        HalOperationResult<T> Authenticate<T>(WebDriverOperationData operationData, AuthenticateAccountRequest request)
            where T : IOperationResponse;
        HalOperationResult<T> EnterTwoFactorAuthenticationCode<T>(WebDriverOperationData operationData, TwoFactorAuthenticationRequest request)
            where T : IOperationResponse;
    }
}
