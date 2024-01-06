using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.EDI;
using ESD_EDI_BE.Models.Dtos;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Q1ManagementController : ControllerBase
    {

        private readonly IQ1ManagementService _q1ManagementService;

        public Q1ManagementController(IQ1ManagementService q1ManagementService)
        {
            _q1ManagementService = q1ManagementService;

        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.Q1MANAGEMENT_READ)]
        public async Task<IActionResult> Get([FromQuery] PPORTAL_QUAL01_INFODto model)
        {
            return Ok(await _q1ManagementService.Get(model));
        }

        [HttpPost("create")]
        [PermissionAuthorization(PermissionConst.Q1MANAGEMENT_CREATE)]
        public async Task<IActionResult> Create([FromBody] PPORTAL_QUAL01_INFODto model)
        {
            var result = await _q1ManagementService.Create(model);
            return Ok(result);
        }

        [HttpPost("create-by-excel")]
        [PermissionAuthorization(PermissionConst.Q1MANAGEMENT_CREATE)]
        public async Task<IActionResult> CreateByExcel([FromBody] List<PPORTAL_QUAL01_INFODto> model)
        {
            var result = await _q1ManagementService.CreateByExcel(model);
            return Ok(result);
        }

        [HttpPut("update")]
        [PermissionAuthorization(PermissionConst.Q1MANAGEMENT_UPDATE)]
        public async Task<IActionResult> Update([FromBody] PPORTAL_QUAL01_INFODto model)
        {
            var result = await _q1ManagementService.Modify(model);
            return Ok(result);
        }

        [HttpDelete("delete")]
        [PermissionAuthorization(PermissionConst.Q1MANAGEMENT_DELETE)]
        public async Task<IActionResult> Delete(long Id)
        {
            var result = await _q1ManagementService.Delete(Id);
            return Ok(result);
        }
    }
}
