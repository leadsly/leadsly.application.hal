using Domain.Models;
using Leadsly.Application.Model;
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
        public FileManager(ILogger<FileManager> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<FileManager> _logger;
        public ResultBase CloneDefaultChromeProfile(string profileDirectoryName, WebDriverOptions options)
        {
            ResultBase result = new ResultBase
            {
                Succeeded = false
            };

            // if there is no way to ship docker containers with our default chrome profile
            //string dir = Directory.GetCurrentDirectory();
            string defaultProfileDir = $"{options.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir}/{options.ChromeProfileConfigOptions.DefaultChromeProfileName}";
            if (Directory.Exists(defaultProfileDir) == false)
            {
                _logger.LogError("Could not locate {defaultProfileDir}", defaultProfileDir);
                result.Failures.Add(new()
                {
                    Reason = "Failed to locate directory",
                    Detail = $"Failed to locate {defaultProfileDir}"
                });
                return result;
            }

            string newProfileDir = Path.Combine(options.ChromeProfileConfigOptions.DefaultChromeUserProfilesDir, profileDirectoryName);

            WalkDirectoryTree(new DirectoryInfo(defaultProfileDir), newProfileDir);

            result = HandleAnyErrors();
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private ResultBase HandleAnyErrors()
        {
            ResultBase result = new ResultBase();
            if (_log.Count > 0)
            {
                int count = _log.Count;
                _logger.LogWarning("Cloning default chrome profile encountered some issues. Error logs detected {count}", count);
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

        private static void WalkDirectoryTree(DirectoryInfo source, string target)
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
                    WalkDirectoryTree(dirInfo, newTarget);
                }
            }
        }
    }
}
