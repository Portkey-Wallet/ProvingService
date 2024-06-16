using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portkey.JwtProof;

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
            var pk =
                "rr9RI_trqotRBIi9qTgoYPE4FnjvhIOoolHhi3Lxw3FqK-8vYwczsVEsTzVek3MAUdNNILhJozKN1Snirlrr_albKitJFJ1SbjJe2HsNeZu96TrWMNmWCCUOXK0ecEVdw8gqfGOE-_VvKaIVxYGaJpSystRjRXfrT8QHXNYMSI7xqJI9Obw4IUpwYxky6eVtB3zLW4Qz7cH2jFMsSB3uufdvOA-odJXrwZkC2FX0krVtSyCWzazjHMX0zOCckZVZK7xBNjuuElcnZ4IpGHcHDYtyI004WY-ez7_yxyOHGoJd31Omg39DkunpSQxfA87LLGumtd1OROvQzNFZETDqfw";
            var input = InputPreparer.Prepare(request.Jwt, pk, HexStringToByteArray(request.Salt));
            var responseMessage = await SendPostRequest(input);
            var proof = await responseMessage.Content.ReadAsStringAsync();

            var bytes2 = new JwtBase64UrlEncoder().Decode(request.Jwt.Split(".")[1]);
            var jsonValue = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(bytes2));
            var subValue = jsonValue["sub"].ToString();
            // var nonceValue = jsonValue["nonce"].ToString();
            var salt = request.Salt;
            var saltBytes = HexStringToByteArray(salt);
            var identifierHash = Helpers.Helper.GetHash(Encoding.UTF8.GetBytes(subValue), saltBytes);

            return new ProveResponse
            {
                Proof = proof,
                IdentifierHash = identifierHash.ToHex()
            };
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