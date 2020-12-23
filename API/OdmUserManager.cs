﻿using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NETCore.Encrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API
{
    public class OdmUserManager : UserManager<ApplicationUser>
    {
        public OdmUserManager(
            IUserStore<ApplicationUser> store, 
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<ApplicationUser> passwordHasher, 
            IEnumerable<IUserValidator<ApplicationUser>> userValidators,
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<ApplicationUser>> logger,
            IConfiguration configuration)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
                keyNormalizer, errors, services, logger)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        //public override string GenerateNewAuthenticatorKey()
        //{
        //    string originalAuthenticatorKey = base.GenerateNewAuthenticatorKey();

        //    string encryptedKey = EncryptProvider.AESEncrypt(originalAuthenticatorKey, _configuration[ApiConstants.VaultKeys.TwoFactorAuthenticationEncryptionKey]);

        //    return encryptedKey;
        //}

        //public override async Task<string> GetAuthenticatorKeyAsync(ApplicationUser user)
        //{
        //    string databaseKey = await base.GetAuthenticatorKeyAsync(user);

        //    if (databaseKey == null)
        //    {
        //        return null;
        //    }

        //    string originalAuthenticatorKey = EncryptProvider.AESDecrypt(databaseKey, _configuration[ApiConstants.VaultKeys.TwoFactorAuthenticationEncryptionKey]);                

        //    return originalAuthenticatorKey;
        //}

        //protected override string CreateTwoFactorRecoveryCode()
        //{
        //    string originalRecoveryCode = base.CreateTwoFactorRecoveryCode();

        //    string encryptedRecoveryCode = EncryptProvider.AESEncrypt(originalRecoveryCode, _configuration[ApiConstants.VaultKeys.TwoFactorAuthenticationEncryptionKey]);                

        //    return encryptedRecoveryCode;
        //}

        //public override async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(ApplicationUser user, int number)
        //{
        //    IEnumerable<string> tokens = await base.GenerateNewTwoFactorRecoveryCodesAsync(user, number);

        //    string[] generatedTokens = tokens as string[] ?? tokens.ToArray();
        //    if (!generatedTokens.Any())
        //    {
        //        return generatedTokens;
        //    }

        //    return generatedTokens.Select(token => EncryptProvider.AESDecrypt(token, _configuration[ApiConstants.VaultKeys.TwoFactorAuthenticationEncryptionKey]));
        //}

        //public override Task<IdentityResult> RedeemTwoFactorRecoveryCodeAsync(ApplicationUser user, string code)
        //{
        //    if (!string.IsNullOrEmpty(code))
        //    {
        //        code = EncryptProvider.AESEncrypt(code, _configuration[ApiConstants.VaultKeys.TwoFactorAuthenticationEncryptionKey]);
        //    }

        //    return base.RedeemTwoFactorRecoveryCodeAsync(user, code);
        //}
    }
}
