using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using ESD_EDI_BE.Cache;
using Microsoft.Extensions.Options;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Services.Cache;

namespace ESD_EDI_BE.Services.FileWatcher
{

    [SingletonRegistration]
    public class FileWatcherService : IDisposable, IHostedService
    {

        // private readonly ISysCacheService _systemCacheService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IServiceProvider _serviceProvider;

        private readonly string[] _directoriesToMonitor;

        public FileWatcherService(
            IWebHostEnvironment webHostEnvironment
            // , IServiceProvider serviceProvider
            // , ISysCacheService systemCacheService
            )
        {
            _webHostEnvironment = webHostEnvironment;
            // _serviceProvider = serviceProvider;
            // _systemCacheService = systemCacheService;
            Console.WriteLine(_webHostEnvironment.EnvironmentName);

            string webRootPath = _webHostEnvironment.WebRootPath;
            string folderData = Path.Combine(webRootPath, CommonConst.EDI, CommonConst.DATA);
            string folderBackup = Path.Combine(webRootPath, CommonConst.EDI, CommonConst.BACKUP);
            string folderLog = Path.Combine(webRootPath, CommonConst.EDI, CommonConst.LOG);

            string folderHMI = Path.Combine(folderData, CommonConst.HMI);
            string folderLoadCell = Path.Combine(folderData, CommonConst.LOADCELL);
            string folderVision = Path.Combine(folderData, CommonConst.VISION);

            _directoriesToMonitor = new string[] {
                folderVision,
                folderHMI,
                folderLoadCell };

            EnsureFolderExists(folderVision);
            EnsureFolderExists(folderHMI);
            EnsureFolderExists(folderLoadCell);
            EnsureFolderExists(folderBackup);
            EnsureFolderExists(folderLog);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("FileWatcherService is starting.");

            // StartTimer();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("FileWatcherService is stopping.");

            // StopTimer();

            return Task.CompletedTask;
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public void Dispose()
        {
        }
    }
}