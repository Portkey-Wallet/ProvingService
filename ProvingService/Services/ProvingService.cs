using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AElf.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Portkey.JwtProof;
using Portkey.JwtProof.Extensions;
using ProvingService.Controllers;
using ProvingService.Helpers;
using ProvingService.Types;

namespace ProvingService.Services;

public interface IProvingService
{
    Task<(string, string)> ProveAsync(ProveRequest request);
}

public class ProofGenerationException : Exception
{
    public ProofGenerationException(string message) : base(message)
    {
    }
}

public class ProvingService : IProvingService
{
    private readonly ProverServerSettings _proverServerSettings;
    private readonly IJwksService _jwksService;
    private readonly IIdentifierHashService _identifierHashService;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<ProvingService> _logger;

    public ProvingService(IOptionsSnapshot<ProverServerSettings> proverServerSettings,
        IHttpClientFactory clientFactory, IJwksService jwksService, IIdentifierHashService identifierHashService,
        ILogger<ProvingService> logger)
    {
        _clientFactory = clientFactory;
        _proverServerSettings = proverServerSettings.Value;
        _jwksService = jwksService;
        _identifierHashService = identifierHashService;
        _logger = logger;
    }

    public async Task<(string, string)> ProveAsync(ProveRequest request)
    {
        var jwt = Jwt.Parse(request.Jwt);
        var salt = Salt.Parse(request.Salt);
        var pk = await _jwksService.GetKeyAsync(jwt.Kid);
        var input = InputPreparer.Prepare(request.Jwt, pk, salt.Value);

        var responseMessage = await SendPostRequest(input);
        var proof = await responseMessage.Content.ReadAsStringAsync();
        if (proof.Contains("failed"))
        {
            throw new ProofGenerationException("Proof generation failed: " + proof);
        }

        return (proof, _identifierHashService.GenerateIdentifierHash(jwt.Subject, request.Salt.HexStringToByteArray()));
    }

    private async Task<HttpResponseMessage> SendPostRequest(Dictionary<string, IList<string>> payload)
    {
        _logger.LogDebug("Sending post request to prover server. {Payload}", payload);
        var client = _clientFactory.CreateClient();
        var jsonString = JsonConvert.SerializeObject(payload);
        _logger.LogInformation("Sending post request to prover server. {}", jsonString);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_proverServerSettings.Endpoint, content);
        return response;
    }
}