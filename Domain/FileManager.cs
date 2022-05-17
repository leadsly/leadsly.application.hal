using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class FileManager : IFileManager
    {
        private static readonly StringCollection _log = new();
        public FileManager(ILogger<FileManager> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileManager> _logger;

        public List<string> LoadExistingChromeProfiles(string browserPurpose, WebDriverOptions options)
        {
            // load existing chrome profiles
            string chromeProfileDir = $"{options.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir}";
            string leadslyProfile = browserPurpose;
            List<string> leadslyChromeProfileDirectories = Directory.GetDirectories(chromeProfileDir).Where(d => d.EndsWith(leadslyProfile)).ToList();

            return leadslyChromeProfileDirectories;
        }

        public HalOperationResult<T> CloneDefaultChromeProfile<T>(string newChromeProfile, WebDriverOptions options) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            string defaultProfileDir = string.Empty;
            if (_env.IsDevelopment())
            {
                string currentDir = $"{Directory.GetCurrentDirectory()}";
                string profileDir = Path.GetFullPath(Path.Combine(currentDir, @".."));
                defaultProfileDir = $"{profileDir}/{options.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir}/{options.ChromeProfileConfigOptions.DefaultChromeProfileName}";
            }
            else
            {
                defaultProfileDir = $"{ options.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir }/{ options.ChromeProfileConfigOptions.DefaultChromeProfileName}";
            }            
            _logger.LogDebug("Default chrome profile directory is: {defaultProfileDir} " +
                "\r\n This is the chrome profile used to authenticate user. After this point all browser instances will be coping this chrome profile and using the copy to launch browsers", defaultProfileDir);

            if (Directory.Exists(defaultProfileDir) == false)
            {
                _logger.LogError("Could not locate {defaultProfileDir}", defaultProfileDir);
                result.Failures.Add(new()
                {
                    Reason = "Failed to locate directory",
                    Detail = $"Failed to locate {defaultProfileDir}"
                });
                return result;
            };

            string newProfileDir = Path.Combine(options.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir, newChromeProfile);

            _logger.LogInformation("Starting to copy all contents of default chrome profile directory, which is: {defaultProfileDir}", defaultProfileDir);
            WalkDirectoryTree(new DirectoryInfo(defaultProfileDir), newProfileDir, _logger);
            _logger.LogInformation("Completed copying all contents of default chrome profile directory");

            result = HandleAnyErrors<T>();
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> HandleAnyErrors<T>() where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            if (_log.Count > 0)
            {
                int count = _log.Count;
                _logger.LogWarning("Cloning default chrome profile encountered some issues. Number of rrror logs detected {count}", count);
                if (_log.Count <= 5)
                {
                    foreach (string log in _log)
                    {
                        result.Failures.Add(new()
                        {
                            Code = Codes.FILE_CLONING_ERROR,
                            Reason = "Failed to clone some chrome profile files",
                            Detail = log
                        });
                    }
                }
                else
                {
                    result.Failures.Add(new()
                    {
                        Code = Codes.FILE_CLONING_ERROR,
                        Reason = "Failed to clone some chrome profile files",
                        Detail = $"Failed to clone {count} files."
                    });
                }
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private static void WalkDirectoryTree(DirectoryInfo source, string target, ILogger<FileManager> logger)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = source.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                _log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            DirectoryInfo targetDirectory = Directory.CreateDirectory(target);

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    string destFile = Path.Combine(target, fi.Name);
                    File.Copy(fi.FullName, destFile, true);
                }

                // Now find all the subdirectories under this directory.
                subDirs = source.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    string newTarget = Path.Combine(target, dirInfo.Name);
                    WalkDirectoryTree(dirInfo, newTarget, logger);
                }
            }
        }

        public HalOperationResult<T> RemoveDirectory<T>(string directory) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                DirectoryInfo di = new DirectoryInfo(directory);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Directory.Delete(directory);
            }
            catch(Exception ex)
            {
                string dirName = directory;
                _logger.LogError(ex, "Failed to remove directory {dirName}", dirName);
                result.Failures.Add(new()
                {
                    Code = Codes.ERROR,
                    Reason = "Failed to delete directory",
                    Detail = ex.Message
                });
                return result;
            }            

            result.Succeeded = true;
            return result;
        }
    }
}
