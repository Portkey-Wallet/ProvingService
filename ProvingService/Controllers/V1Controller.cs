using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using ProvingService.Services;

namespace ProvingService.Controllers
{
    [ApiController]
    [Route("v1")]
    public class V1Controller : ControllerBase
    {
        private readonly IProvingService _provingService;
        private readonly IVerifyingService _verifyingService;

        public V1Controller(IProvingService provingService, IVerifyingService verifyingService)
        {
            _provingService = provingService;
            _verifyingService = verifyingService;
        }

        [HttpGet("health")]
        public async Task<string> Health()
        {
            return "ok";
        }

        [HttpPost("prove")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProveResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> Prove(ProveRequest request)
        {
            try
            {
                var (proof, identifierHash) = await _provingService.ProveAsync(request);

                return Ok(new ProveResponse
                {
                    Proof = proof,
                    IdentifierHash = identifierHash
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { Error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("verify")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyingResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> Verify(VerifyRequest request)
        {
            try
            {
                var valid = await _verifyingService.VerifyAsync(request);
                return Ok(new VerifyingResponse
                {
                    Valid = valid
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { Error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("verifying-key")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyingKeyResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> VerifyingKey()
        {
            try
            {
                var key = _verifyingService.GetVerifyingKey();
                return Ok(new VerifyingKeyResponse()
                {
                    Key = key
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { Error = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpGet("verifying-key")]
        public async Task<string> GetVerifyingKey()
        {
            return _verifyingService.GetVerifyingKey();
        }
    }
}