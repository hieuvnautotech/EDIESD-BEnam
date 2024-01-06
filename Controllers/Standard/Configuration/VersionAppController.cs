using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http.Extensions;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Cors;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionAppController : ControllerBase
    {
        private readonly IVersionAppService _versionAppService;
        private readonly IJwtService _jwtService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IExpoTokenService _expoTokenService;

        public VersionAppController(
            IVersionAppService versionAppService
            , IJwtService jwtService
            , IWebHostEnvironment webHostEnvironment
            //, IHttpClientFactory httpClientFactory
            //, IExpoTokenService expoTokenService
            )
        {
            _versionAppService = versionAppService;
            _jwtService = jwtService;
            _webHostEnvironment = webHostEnvironment;
            //_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            //_expoTokenService = expoTokenService;
        }

        [HttpGet("get-version-app")]
        public async Task<IActionResult> GetVersionApp()
        {
            var returnData = await _versionAppService.GetAll();
            return Ok(returnData);
        }

        [HttpGet("get-app-by-code/{app_code}")]
        public async Task<IActionResult> GetAppByCode(string app_code)
        {
            var returnData = await _versionAppService.GetAppByCode(app_code);
            return Ok(returnData);
        }

        [HttpPost("update-version-app")]
        public async Task<IActionResult> Update([FromForm] VersionAppDto input)
        {
            ////xử lí lưu file apk
            //var webPath = System.IO.Directory.GetCurrentDirectory();
            //string folder_path = Path.Combine($"{webPath}/Upload/APK");
            //if (!Directory.Exists(folder_path))
            //{
            //    Directory.CreateDirectory(folder_path);
            //}


            //string[] files = Directory.GetFiles(folder_path);
            //foreach (string item in files)
            //{
            //    System.IO.File.Delete(item);
            //}

            //using (var stream = System.IO.File.Create(Path.Combine(folder_path, input.file.FileName)))
            //{
            //    await input.file.CopyToAsync(stream);
            //}
            //input.name_file = input.file.FileName;

            //var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //var userId = _jwtService.ValidateToken(token);
            // var userId = await _jwtService.ValidateTokenAsync(token);
            //input.createdBy = long.Parse(userId);

            //var returnData = new ResponseModel<VersionAppDto?>();
            //var result = await _versionAppService.Modify(input);

            //switch (result)
            //{
            //    case StaticReturnValue.SYSTEM_ERROR:
            //        returnData.HttpResponseCode = 500;
            //        break;
            //    case StaticReturnValue.SUCCESS:
            //        returnData = await _versionAppService.GetAll();
            //        break;
            //    default:
            //        returnData.HttpResponseCode = 400;
            //        break;
            //}

            //returnData.ResponseMessage = result;
            //return Ok(returnData);
            var returnData = new ResponseModel<IEnumerable<VersionAppDto>>();
            if (string.IsNullOrEmpty(input.app_code))
            {
                return Ok(returnData);
            }

            if (input.file != null)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                string folderRoot = Path.Combine(webRootPath, "APK");

                if (!Directory.Exists(folderRoot))
                {
                    Directory.CreateDirectory(folderRoot);
                }

                string folderPath = Path.Combine(folderRoot, input.app_code);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string[] files = Directory.GetFiles(folderPath);
                foreach (string item in files)
                {
                    System.IO.File.Delete(item);
                }

                using (var stream = System.IO.File.Create(Path.Combine(folderPath, input.file.FileName)))
                {
                    await input.file.CopyToAsync(stream);
                }

                input.name_file = input.file.FileName;
            }

            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            input.createdBy = long.Parse(userId);

            var result = await _versionAppService.Modify(input);

            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData = await _versionAppService.GetAll();
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            returnData.ResponseMessage = result;
            return Ok(returnData);

        }


        [HttpGet("download-version-app/{app_code}")]
        public async Task<IActionResult> DownloadFile(string app_code)
        {
            //try
            //{
            //    //Get filename
            //    var returnData = new ResponseModel<VersionAppDto?>();
            //    returnData = await _versionAppService.GetAll();
            //    string fileName = returnData.Data.name_file;

            //    var webPath = System.IO.Directory.GetCurrentDirectory();
            //    string folder_path = Path.Combine($"{webPath}/Upload/APK/" , fileName);

            //    return PhysicalFile(folder_path, "application/vnd.android.package-archive", Path.GetFileName(folder_path));
            //}
            //catch
            //{
            //    return BadRequest();
            //}

            try
            {
                var returnData = new ResponseModel<VersionAppDto?>();
                returnData = await _versionAppService.GetAppByCode(app_code);
                string fileName = returnData.Data.name_file;

                return Ok(Path.Combine("APK", app_code, fileName));
            }
            catch (Exception)
            {
                return BadRequest();
                throw;
            }
        }
        //[HttpGet("download-file")]
        //public async Task<IActionResult> DownloadFile(string url, string extension)
        //{
        //    string fileName = $"{DateTime.Now.Ticks}.{extension}";
        //    var httpClient = new HttpClient();
        //    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        //    var stream = await response.Content.ReadAsStreamAsync();

        //    if (response.Content.Headers.TryGetValues("Content-Length", out var contentLengthValues) && stream != null)
        //    {
        //        long.TryParse(contentLengthValues.First(), out var contentLength);
        //        var fileContentResult = new FileContentResult(stream.ToArray(), "application/octet-stream")
        //        {
        //            FileDownloadName = fileName
        //        };
        //        fileContentResult.FileDownloadName = fileName;
        //        fileContentResult.ContentLength = contentLength;
        //        return fileContentResult;
        //    }

        //    return File(stream, "application/octet-stream", fileName);
        //}
    }
}
