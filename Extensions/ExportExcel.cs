using Microsoft.AspNetCore.Hosting;
using MiniExcelLibs;

namespace ESD_EDI_BE.Extensions
{
    public static class ExportExcel
    {
        public async static Task<string> SaveFile<T>(IWebHostEnvironment _webHostEnvironment, string fileName, string filePath, T sheets)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(webRootPath, "Export");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            DeleteFilesCreated5MinutesAgo(folderPath);
            var memoryStream = new MemoryStream();
            try
            {
                MiniExcel.SaveAsByTemplate(memoryStream, filePath, sheets);
            }
            catch (Exception ex)
            {

                throw;
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileNameDownload = $"{fileName}-{DateTime.Now.ToString("yyyyMMddhhmmss")}.xlsx";

            var downloadLink = Path.Combine(folderPath, fileNameDownload);
            using (var stream = File.Create(downloadLink))
            {
                await memoryStream.CopyToAsync(stream);
            }
            return Path.Combine("Export", fileNameDownload);
        }

        public async static Task<string> SaveFileWithoutTemplate<T>(IWebHostEnvironment _webHostEnvironment, string fileName, string filePath, T sheets)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(webRootPath, "Export");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            DeleteFilesCreated5MinutesAgo(folderPath);
            var memoryStream = new MemoryStream();
            try
            {
                MiniExcel.SaveAs(memoryStream, sheets);
            }
            catch (Exception ex)
            {

                throw;
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileNameDownload = $"{fileName}-{DateTime.Now.ToString("yyyyMMddhhmmss")}.xlsx";

            var downloadLink = Path.Combine(folderPath, fileNameDownload);
            using (var stream = File.Create(downloadLink))
            {
                await memoryStream.CopyToAsync(stream);
            }
            return Path.Combine("Export", fileNameDownload);
        }
        private static void DeleteFilesCreated5MinutesAgo(string directoryPath)
        {
            DateTime fiveMinutesAgo = DateTime.Now.AddMinutes(-5);

            DirectoryInfo directory = new DirectoryInfo(directoryPath);

            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.CreationTime < fiveMinutesAgo)
                {
                    file.Delete();
                }
            }
        }
    }
}
