using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/expo-token")]
    [ApiController]
    [AllowAll]
    public class ExpoTokenController : ControllerBase
    {
        private readonly IExpoTokenService _expoTokenService;

        public ExpoTokenController(IExpoTokenService expoTokenService)
        {
            _expoTokenService = expoTokenService;
        }

        [HttpGet("get-expo-tokens")]
        public async Task<IActionResult> GetActive()
        {
            return Ok(await _expoTokenService.GetActive());
        }

        [HttpPost("create-expo-token")]
        public async Task<IActionResult> Create([FromBody] ExpoTokenDto model)
        {
            var result = await _expoTokenService.Create(model);
            return Ok(result);
        }
    }
}
