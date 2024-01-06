using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Models.Validators;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    //[RoleAuthorization(RoleConst.ROOT)]
    public class CommonMasterController : ControllerBase
    {
        private readonly ICommonMasterService _commonMasterService;
        private readonly IJwtService _jwtService;

        public CommonMasterController(ICommonMasterService commonMasterService, IJwtService jwtService)
        {
            _commonMasterService = commonMasterService;
            _jwtService = jwtService;
        }

        [HttpGet]
        [PermissionAuthorization(PermissionConst.COMMONMASTER_READ)]
        public async Task<IActionResult> Get([FromQuery] PageModel pageInfo, string? keyWord, bool showDelete = true)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var roleName = _jwtService.GetRolenameFromToken(token);
            var returnData = await _commonMasterService.Get(pageInfo, roleName, keyWord, showDelete);
            return Ok(returnData);
        }

        [HttpPost("create-commonmaster")]
        [PermissionAuthorization(PermissionConst.COMMONMASTER_CREATE)]
        public async Task<IActionResult> Create(CommonMasterDto model)
        {
            var returnData = new ResponseModel<CommonMasterDto?>();
            var validator = new CommonMasterValidator();
            var validateResults = validator.Validate(model);

            if (!validateResults.IsValid)
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = validateResults.Errors[0].ToString();
                return Ok(returnData);
            }

            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.createdBy = long.Parse(userId);
            var result = await _commonMasterService.Create(model);

            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SUCCESS:
                    returnData = await _commonMasterService.GetByName(model.commonMasterName);
                    returnData.ResponseMessage = result;
                    break;
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpPut("modify-commonmaster")]
        [PermissionAuthorization(PermissionConst.COMMONMASTER_UPDATE)]
        public async Task<IActionResult> Modify(CommonMasterDto model)
        {
            var returnData = new ResponseModel<CommonMasterDto?>();
            var validator = new CommonMasterValidator();
            var validateResults = validator.Validate(model);

            if (!validateResults.IsValid)
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = validateResults.Errors[0].ToString();
                return Ok(returnData);
            }

            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // var userId = _jwtService.ValidateToken(token);
            var userId = await _jwtService.ValidateTokenAsync(token);
            model.modifiedBy = long.Parse(userId);
            var result = await _commonMasterService.Modify(model);


            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SYSTEM_ERROR:
                    returnData.HttpResponseCode = 500;
                    break;
                case StaticReturnValue.SUCCESS:
                    returnData = await _commonMasterService.GetByCode(model.commonMasterCode);
                    returnData.ResponseMessage = result;
                    break;
                default:
                    returnData.HttpResponseCode = 400;
                    break;
            }

            return Ok(returnData);
        }

        [HttpPut("delete-commonmaster")]
        [PermissionAuthorization(PermissionConst.COMMONMASTER_DELETE)]
        public async Task<IActionResult> Delete([FromBody] CommonMasterDto model)
        {
            try
            {
                var result = await _commonMasterService.Delete(model.commonMasterCode, (bool)model.isActived, model.row_version);
                var returnData = new ResponseModel<CommonMasterDto?>
                {
                    ResponseMessage = result
                };
                switch (result)
                {
                    case StaticReturnValue.COMMONDETAIL_EXISTED:
                        returnData.ResponseMessage = result;
                        returnData.HttpResponseCode = 300;
                        break;
                    case StaticReturnValue.SYSTEM_ERROR:
                        returnData.HttpResponseCode = 500;
                        break;
                    case StaticReturnValue.SUCCESS:
                        break;
                    default:
                        returnData.HttpResponseCode = 400;
                        break;
                }

                return Ok(returnData);
            }
            catch (Exception e)
            {

                throw;
            }

        }

    }
}
