using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESD_EDI_BE.Extensions
{
    public static class CommonFunction
    {
        private static void CreateFolderIfNotExists(string fileDirectory)
        {
            if (!Directory.Exists(fileDirectory) && !string.IsNullOrEmpty(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }
        }

        public static void SaveHangfireLog(string filePath, string message)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string parentFolderPath = Path.GetDirectoryName(filePath);
                CreateFolderIfNotExists(parentFolderPath);

                using StreamWriter writer = new(filePath, true);
                writer.WriteLine(message);
            }

        }
    }
}