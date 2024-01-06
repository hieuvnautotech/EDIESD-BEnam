using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Models.Validators;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Controllers.Standard.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    // [RoleAuthorization(RoleConst.ROOT)]
    public class CommonDetailController : ControllerBase
    {
        private readonly ICommonDetailService _commonDetailService;
        private readonly IJwtService _jwtService;

        public CommonDetailController(ICommonDetailService commonDetailService, IJwtService jwtService)
        {
            _commonDetailService = commonDetailService;
            _jwtService = jwtService;
        }

        [HttpGet("getall-by-masterCode")]
        [PermissionAuthorization(PermissionConst.COMMONDETAIL_READ)]
        public async Task<IActionResult> GetByCommonMasterCode([FromQuery] PageModel pageInfo, string commonMasterCode, bool? isActived, string keyWord)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            return Ok(await _commonDetailService.GetByCommonMasterCode(pageInfo, long.Parse(userId), commonMasterCode, isActived, keyWord));
        }


        [HttpPost("create-commondetail")]
        [PermissionAuthorization(PermissionConst.COMMONDETAIL_CREATE)]
        public async Task<IActionResult> Create(CommonDetailDto model)
        {
            var returnData = new ResponseModel<CommonDetailDto>();
            var validator = new CommonDetailValidator();
            var validateResults = validator.Validate(model);

            if (!validateResults.IsValid)
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = validateResults.Errors[0].ToString();
                return Ok(returnData);
            }

            model.commonDetailId = AutoId.AutoGenerate();
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // model.createdBy = _jwtService.GetUserIdFromToken(token);
            model.createdBy = _jwtService.GetUserIdFromToken(token);

            var result = await _commonDetailService.Create(model);

            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SUCCESS:
                    returnData = await _commonDetailService.GetById(model.commonDetailId);
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

        [HttpPut("modify-commondetail")]
        [PermissionAuthorization(PermissionConst.COMMONDETAIL_UPDATE)]
        public async Task<IActionResult> Modify(CommonDetailDto model)
        {
            var returnData = new ResponseModel<CommonDetailDto?>();
            var validator = new CommonDetailValidator();
            var validateResults = validator.Validate(model);

            if (!validateResults.IsValid)
            {
                returnData.HttpResponseCode = 400;
                returnData.ResponseMessage = validateResults.Errors[0].ToString();
                return Ok(returnData);
            }

            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // model.modifiedBy = _jwtService.GetUserIdFromToken(token);
            model.modifiedBy = _jwtService.GetUserIdFromToken(token);

            var result = await _commonDetailService.Modify(model);
            returnData.ResponseMessage = result;
            switch (result)
            {
                case StaticReturnValue.SUCCESS:
                    returnData = await _commonDetailService.GetById(model.commonDetailId);
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

        [HttpPut("delete-reuse-commondetail")]
        //[PermissionAuthorization(PermissionConst.USER_UPDATE)]
        public async Task<IActionResult> DeleteReuse(CommonDetailDto model)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            // model.modifiedBy = _jwtService.GetUserIdFromToken(token);
            model.modifiedBy = _jwtService.GetUserIdFromToken(token);

            var result = await _commonDetailService.DeleteReuse(model);

            var returnData = new ResponseModel<CommonMasterDto?>();
            returnData.ResponseMessage = result;
            switch (result)
            {
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
    }
}
