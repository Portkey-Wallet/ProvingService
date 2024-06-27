using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using ProvingService.Application.Contracts.HashMapping;

namespace ProvingService.Controllers;

[ApiController]
[Route("v1/hash-mapping")]
[FeatureGate("UsePoseidon")]
public class V1HashMappingController : ControllerBase
{
    private readonly IProvingServiceForHashMapping _provingServiceForHashMapping;
    private readonly IVerifyingServiceForHashMapping _verifyingServiceForHashMapping;

    public V1HashMappingController(IProvingServiceForHashMapping provingServiceForHashMapping,
        IVerifyingServiceForHashMapping verifyingServiceForHashMapping)
    {
        _provingServiceForHashMapping = provingServiceForHashMapping;
        _verifyingServiceForHashMapping = verifyingServiceForHashMapping;
    }

    [HttpGet("health")]
    public async Task<string> Health()
    {
        return await Task.FromResult("ok");
    }

    [HttpPost("prove")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HashMappingProveResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> ProveHashMapping(HashMappingProveRequest request)
    {
        try
        {
            var output = await _provingServiceForHashMapping.ProveAsync(new ProveInputForHashMapping
            {
                Subject = request.Subject,
                Salt = request.Salt
            });

            return Ok(new HashMappingProveResponse()
            {
                PoseidonIdHash = output.PoseidonIdHash,
                Sha256IdHash = output.Sha256IdHash,
                Proof = output.Proof,
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
    public async Task<IActionResult> VerifyHashMapping(HashMappingVerifyRequest request)
    {
        try
        {
            var valid = await _verifyingServiceForHashMapping.VerifyAsync(new VerifyInputForHashMapping()
            {
                PoseidonIdHash = request.PoseidonIdHash,
                Sha256IdHash = request.Sha256IdHash,
                Proof = request.Proof,
            });
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
            var key = _verifyingServiceForHashMapping.GetVerifyingKey();
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
        return _verifyingServiceForHashMapping.GetVerifyingKey();
    }
}