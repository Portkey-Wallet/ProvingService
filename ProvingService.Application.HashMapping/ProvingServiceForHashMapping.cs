using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProvingService.Application.Contracts.HashMapping;
using ProvingService.Domain.Common.Extensions;
using ProvingService.Domain.HashMapping;
using ProvingService.Domain.PoseidonHash;
using ProvingService.Domain.Sha256Hash;

namespace ProvingService.Application.HashMapping;

public class ProvingServiceForHashMapping : IProvingServiceForHashMapping
{
    private readonly ProverServerSettingsForHashMapping _proverServerSettings;
    private readonly IHttpClientFactory _clientFactory;
    private readonly PoseidonIdentifierHashService _poseidonIdentifierHashService;
    private readonly Sha256IdentifierHashService _sha256IdentifierHashService;
    private readonly ILogger<ProvingServiceForHashMapping> _logger;

    public ProvingServiceForHashMapping(
        IOptionsSnapshot<ProverServerSettingsForHashMapping> proverServerSettings,
        IHttpClientFactory clientFactory, PoseidonIdentifierHashService poseidonIdentifierHashService,
        Sha256IdentifierHashService sha256IdentifierHashService,
        ILogger<ProvingServiceForHashMapping> logger)
    {
        _proverServerSettings = proverServerSettings.Value;
        _clientFactory = clientFactory;
        _poseidonIdentifierHashService = poseidonIdentifierHashService;
        _sha256IdentifierHashService = sha256IdentifierHashService;
        _logger = logger;
    }

    public async Task<ProveOutputForHashMapping> ProveAsync(ProveInputForHashMapping input)
    {
        var poseidonIdHash =
            _poseidonIdentifierHashService.GenerateIdentifierHash(input.Subject, input.Salt.HexStringToByteArray());
        var sha256IdHash =
            _sha256IdentifierHashService.GenerateIdentifierHash(input.Subject, input.Salt.HexStringToByteArray());

        var preparedInput = InputPreparer.Prepare(input.Subject, input.Salt.HexStringToByteArray());

        var responseMessage = await SendPostRequest(preparedInput);
        var proof = await responseMessage.Content.ReadAsStringAsync();
        if (proof.Contains("failed"))
        {
            throw new ProofGenerationException("Proof generation failed: " + proof);
        }

        return await Task.FromResult(new ProveOutputForHashMapping
        {
            PoseidonIdHash = poseidonIdHash,
            Sha256IdHash = sha256IdHash,
            Proof = proof
        });
    }

    private async Task<HttpResponseMessage> SendPostRequest(Dictionary<string, IList<string>> payload)
    {
        var client = _clientFactory.CreateClient();
        var jsonString = JsonConvert.SerializeObject(payload);
        _logger.LogInformation("Sending post request to prover server. {}", jsonString);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_proverServerSettings.Endpoint, content);
        return response;
    }
}