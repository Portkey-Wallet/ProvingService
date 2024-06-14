using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ProvingService.Controllers
{
    [ApiController]
    [Route("v1")]
    public class V1Controller : ControllerBase
    {
        private readonly ProverServerSettings _proverServerSettings;

        public V1Controller(IOptionsSnapshot<ProverServerSettings> proverServerSettings)
        {
            _proverServerSettings = proverServerSettings.Value;
        }

        [HttpGet("health")]
        public async Task<string> Health()
        {
            return "ok";
        }

        [HttpPost("prove")]
        public async Task<ProveResponse> Prove(ProveRequest request)
        {
            return new ProveResponse
            {
                Proof = _proverServerSettings.Endpoint,
                IdentifierHash = "def"
            };
        }
    }
}