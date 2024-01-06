using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.EDI;
using ESD_EDI_BE.Models.Dtos;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Q2ManagementController : ControllerBase
    {

        private readonly IQ2ManagementService _q2ManagementService;
        private readonly ICommonDetailService _commonDetailService;

        public Q2ManagementController(
            IQ2ManagementService q2ManagementService
            , ICommonDetailService commonDetailService
            )
        {
            _q2ManagementService = q2ManagementService;
            _commonDetailService = commonDetailService;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.Q2MANAGEMENT_READ)]
        public async Task<IActionResult> Get(int page, int pageSize, string ITEM_CODE, string TRAND_TP, DateTime? StartDate, DateTime? EndDate)
        {
            return Ok(await _q2ManagementService.Get(page, pageSize, ITEM_CODE, TRAND_TP, StartDate, EndDate));
        }

        [HttpGet("get-send-type")]
        [PermissionAuthorization(PermissionConst.Q2MANAGEMENT_READ)]
        public async Task<IActionResult> GetSendType(string commonMasterCode)
        {
            //var data = await _sysCacheService.GetPolicies();
            return Ok(await _commonDetailService.GetForSelectByMasterCode(commonMasterCode));
        }
    }
}
