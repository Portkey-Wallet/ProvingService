using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.JwtProof;
using ProvingService.Services;
using ProvingService.Types;

namespace ProvingService.Controllers
{
    [ApiController]
    [Route("v1")]
    public class V1Controller : ControllerBase
    {
        private readonly ProverServerSettings _proverServerSettings;
        private readonly IJwksService _jwksService;

        public V1Controller(IOptionsSnapshot<ProverServerSettings> proverServerSettings, IJwksService jwksService)
        {
            _proverServerSettings = proverServerSettings.Value;
            _jwksService = jwksService;
        }

        [HttpGet("health")]
        public async Task<string> Health()
        {
            return "ok";
        }

        [HttpPost("prove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> Prove(ProveRequest request)
        {
            try
            {
                var jwt = Jwt.Parse(request.Jwt);
                var pk = await _jwksService.GetKeyAsync(jwt.Kid);
                var input = InputPreparer.Prepare(request.Jwt, pk, HexStringToByteArray(request.Salt));
                var responseMessage = await SendPostRequest(input);
                var proof = await responseMessage.Content.ReadAsStringAsync();

                var salt = request.Salt;
                var saltBytes = HexStringToByteArray(salt);
                var identifierHash = Helpers.HashHelper.GetHash(Encoding.UTF8.GetBytes(jwt.Subject), saltBytes);

                return Ok(new ProveResponse
                {
                    Proof = proof,
                    IdentifierHash = identifierHash.ToHex()
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { Error = $"An error occurred: {ex.Message}" });
            }
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            var length = hex.Length;
            var bytes = new byte[length / 2];
            for (var i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }


        private async Task<HttpResponseMessage> SendPostRequest(Dictionary<string, IList<string>> payload)
        {
            using var client = new HttpClient();
            var jsonString = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_proverServerSettings.Endpoint, content);
            return response;
        }
    }
}