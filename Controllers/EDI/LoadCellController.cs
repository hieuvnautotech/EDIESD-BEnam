using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.EDI;
using Microsoft.AspNetCore.Mvc;

namespace ESD_EDI_BE.Controllers.EDI
{
    [Route("api/loadcell")]
    [ApiController]
    public class LoadCellController : ControllerBase
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMachineService _machineService;

        public LoadCellController(IWebHostEnvironment webHostEnvironment, IMachineService machineService)
        {
            _webHostEnvironment = webHostEnvironment;
            _machineService = machineService;
        }

        [HttpPost("create-loadcell-data")]
        public async Task<IActionResult> CreateLoadCellData([FromForm] LoadCellDataDto input)
        {

            var returnData = new ResponseModel<LoadCellDataDto>
            {
                ResponseMessage = "Success"
            };
            var machineData = await _machineService.GetByCode(input.ESDMachineCode);

            if (machineData.Data != null)
            {
                if (input.file != null)
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string folderData = Path.Combine(webRootPath, CommonConst.EDI, CommonConst.DATA);
                    string folderLoadCell = Path.Combine(folderData, CommonConst.LOADCELL);

                    if (!Directory.Exists(folderLoadCell))
                    {
                        Directory.CreateDirectory(folderLoadCell);
                    }

                    using var stream = System.IO.File.Create(Path.Combine(folderLoadCell, input.file.FileName));
                    await input.file.CopyToAsync(stream);
                }
            }

            else
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = "Không tìm thấy Machine Code";
            }

            return Ok(returnData);
        }
    }
}
