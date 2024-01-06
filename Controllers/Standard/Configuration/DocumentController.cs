using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;
using System.Text;
using Newtonsoft.Json;
using ESD_EDI_BE.RabbitMQ.PushToQueue;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICustomService _customService;
        private readonly ICommonDetailService _commonDetailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DocumentController(
            IWebHostEnvironment webHostEnvironment
            , ICommonDetailService commonDetailService
            , IDocumentService documentService
            , ICustomService customService
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _commonDetailService = commonDetailService;
            _documentService = documentService;
            _customService = customService;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.DOCUMENT_READ)]
        public async Task<IActionResult> Get([FromQuery] PageModel pageInfo, string keyWord, string documentLanguage)
        {
            var data = await _documentService.Get(pageInfo, keyWord, documentLanguage);
            return Ok(data);
        }

        [HttpGet("get-menu-component")]
        [AllowAll]
        public async Task<IActionResult> GetMenuComponent()
        {
            string column = "menuComponent, menuName";
            string table = "sysTbl_Menu";
            string where = "isnull(menuComponent, '') <> ''";

            return Ok(await _customService.GetForSelect<dynamic>(column, table, where, ""));
        }

        [HttpGet("get-language")]
        [AllowAll]
        public async Task<IActionResult> GetLanguage()
        {
            return Ok(await _commonDetailService.GetForSelectByMasterCode("000"));
        }

        [HttpPost("create-document")]
        [PermissionAuthorization(PermissionConst.DOCUMENT_CREATE)]
        public async Task<IActionResult> Create([FromForm] DocumentDto model)
        {
            var returnData = new ResponseModel<DocumentDto?>();
            //xử lí lưu file 
            if (model.file != null)
            {
                string ext = Path.GetExtension(model.file.FileName);
                if (ext.ToLower() == ".pdf")
                {
                    var webPath = _webHostEnvironment.WebRootPath;
                    string folder_path = Path.Combine($"{webPath}/Document/{model.documentLanguage}");
                    if (!Directory.Exists(folder_path))
                    {
                        Directory.CreateDirectory(folder_path);
                    }


                    string filename = model.menuComponent + "-" + model.documentLanguage + ext;
                    using (var stream = System.IO.File.Create(Path.Combine(folder_path, filename)))
                    {
                        await model.file.CopyToAsync(stream);
                    }
                    model.urlFile = filename;
                    model.documentId = AutoId.AutoGenerate();
                    returnData = await _documentService.Create(model);
                }
                else
                {
                    returnData.HttpResponseCode = 400;
                    returnData.ResponseMessage = "general.pdfOnly";
                }
            }
            else
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = "general.not_select_file";
            }

            return Ok(returnData);
        }

        [HttpPut("modify-document")]
        [PermissionAuthorization(PermissionConst.DOCUMENT_UPDATE)]
        public async Task<IActionResult> Modify([FromForm] DocumentDto model)
        {
            var returnData = new ResponseModel<DocumentDto?>();
            if (model.file != null)
            {
                string ext = Path.GetExtension(model.file.FileName);
                if (ext.ToLower() == ".pdf")
                {
                    var webPath = _webHostEnvironment.WebRootPath;
                    string folder_path = Path.Combine($"{webPath}/Document/{model.documentLanguage}");
                    if (!Directory.Exists(folder_path))
                    {
                        Directory.CreateDirectory(folder_path);
                    }

                    string filename = model.menuComponent + "-" + model.documentLanguage + ext;
                    using (var stream = System.IO.File.Create(Path.Combine(folder_path, filename)))
                    {
                        await model.file.CopyToAsync(stream);
                    }
                    model.urlFile = filename;
                }
                else
                {
                    returnData.HttpResponseCode = 400;
                    returnData.ResponseMessage = "general.pdfOnly";

                    return Ok(returnData);
                }
            }

            returnData = await _documentService.Modify(model);
            return Ok(returnData);
        }

        [HttpGet("download/{menuComponent}/{language}")]
        [AllowAll]
        public async Task<IActionResult> DownloadFile(string menuComponent, string language)
        {
            try
            {
                var result = await _documentService.GetByComponent(menuComponent, language);
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("delete-document")]
        [PermissionAuthorization(PermissionConst.DOCUMENT_DELETE)]
        public async Task<IActionResult> Delete(DocumentDto model)
        {
            var deleteDocument = await _documentService.GetById(model.documentId);
            var result = await _documentService.Delete(model);

            var returnData = new ResponseModel<DocumentDto>
            {
                ResponseMessage = result,
                Data = deleteDocument.Data
            };

            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    if (_webHostEnvironment.EnvironmentName == CommonConst.STAGING)
                    {

                        returnData.Data.RabbitMQType = CommonConst.RABBITMQ_TYPE_DELETE;
                        byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(returnData.Data));
                        RabbitMqPush.PublishToRabbitMQ(exchangeName: CommonConst.AUTONSI_EXCHANGE,
                                                                                   routingKey: CommonConst.AUTONSI_QUEUE_DOCUMENT,
                                                                                   queueName: CommonConst.AUTONSI_QUEUE_DOCUMENT,
                                                                                   messageBody: messageBody);
                    }
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }
    }
}
