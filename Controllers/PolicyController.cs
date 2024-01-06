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
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Models.Redis;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolicyController : ControllerBase
    {

        private readonly IPolicyService _policyService;
        private readonly IJwtService _jwtService;
        private readonly ISysCacheService _sysCacheService;
        private readonly ICommonDetailService _commonDetailService;

        public PolicyController(
            IPolicyService policyService
            , IJwtService jwtService
            , ISysCacheService sysCacheService
            , ICommonDetailService commonDetailService
            )
        {
            _policyService = policyService;
            _jwtService = jwtService;
            _sysCacheService = sysCacheService;
            _commonDetailService = commonDetailService;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.Q2POLICY_READ)]
        public async Task<IActionResult> Get([FromQuery] PPORTAL_QUAL02_POLICYDto model)
        {
            //var data = await _sysCacheService.GetPolicies();
            return Ok(await _policyService.Get(model));
        }

        [HttpGet("get-send-type")]
        [PermissionAuthorization(PermissionConst.Q2POLICY_READ)]
        public async Task<IActionResult> GetSendType(string commonMasterCode)
        {
            //var data = await _sysCacheService.GetPolicies();
            return Ok(await _commonDetailService.GetForSelectByMasterCode(commonMasterCode));
        }

        [HttpPost("create")]
        [PermissionAuthorization(PermissionConst.Q2POLICY_CREATE)]
        public async Task<IActionResult> Create([FromBody] PPORTAL_QUAL02_POLICYDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userId);
            model.Id = AutoId.AutoGenerate();
            var result = await _policyService.Create(model);
            if (result.Data != null)
            {
                try
                {
                    await _sysCacheService.SetPoliciesToRedis(AutoMapperConfig<PPORTAL_QUAL02_POLICYDto, PPORTAL_QUAL02_POLICY_Redis>.Map(result.Data));
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            return Ok(result);
        }

        [HttpPut("update")]
        [PermissionAuthorization(PermissionConst.Q2POLICY_UPDATE)]
        public async Task<IActionResult> Update([FromBody] PPORTAL_QUAL02_POLICYDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userId);
            var result = await _policyService.Modify(model);
            if (result.Data != null)
            {
                await _sysCacheService.SetPoliciesToRedis(AutoMapperConfig<PPORTAL_QUAL02_POLICYDto, PPORTAL_QUAL02_POLICY_Redis>.Map(result.Data));
            }
            return Ok(result);
        }

        [HttpPut("delete")]
        [PermissionAuthorization(PermissionConst.Q2POLICY_DELETE)]
        public async Task<IActionResult> Delete([FromBody] PPORTAL_QUAL02_POLICYDto model)
        {
            var result = await _policyService.Delete(model);
            if (result.HttpResponseCode == 200)
            {
                try
                {
                    await _sysCacheService.RemovePoliciesFromRedis(model);
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            return Ok(result);
        }
    }
}
