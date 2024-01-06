using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using Microsoft.AspNetCore.Mvc;
using ESD_EDI_BE.CustomAttributes;

namespace ESD_EDI_BE.Controllers.EDI
{
    [Route("api/log-file")]
    [ApiController]
    public class LogFileController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LogFileController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAll]
        public async Task<IActionResult> Get(DateTime searchDate)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string folderLog = Path.Combine(webRootPath, CommonConst.EDI, CommonConst.LOG);

            string dateStr = searchDate.ToString("yy-MM-dd");

            string filePath = Path.Combine(folderLog, dateStr, $"{dateStr}.txt");

            var returnData = new List<LogDto>();

            if (System.IO.File.Exists(filePath))
            {

                using var streamRdr = new StreamReader(filePath);

                var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Encoding = Encoding.UTF8, // File sử dụng encoding UTF-8.
                    Delimiter = "*", // Ký tự phân cách giữa các trường là dấu phẩy.
                    HasHeaderRecord = false
                };

                using var csv = new CsvReader(streamRdr, configuration);

                var records = csv.GetRecordsAsync<LogDto>();

                await foreach (var record in records)
                {
                    var logDto = record;
                    if (logDto != null)
                    {
                        returnData.Add(logDto);
                    }
                }
            }

            return Ok(returnData);
        }
    }
}