using Groth16.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProvingService.Application.Contracts.HashMapping;
using ProvingService.Domain.Common.Extensions;

namespace ProvingService.Application.HashMapping;

public class VerifyingException : Exception
{
    public VerifyingException(string message) : base(message)
    {
    }
}

/// <summary>
/// For convenience, the VerifyingServiceForHashMapping class is used to verify the proof of the user.
/// </summary>
public class VerifyingServiceForHashMapping : IVerifyingServiceForHashMapping
{
    private CircuitSettingsForHashMapping _circuitSettings;
    private string? _verifyingKey;

    public string GetVerifyingKey()
    {
        if (_verifyingKey != null) return _verifyingKey;
        using var prover = Prover.Create(_circuitSettings.WasmPath, _circuitSettings.R1csPath,
            _circuitSettings.ZkeyPath);
        _verifyingKey = prover.ExportVerifyingKeyBn254();

        return _verifyingKey;
    }

    public VerifyingServiceForHashMapping(IOptionsSnapshot<CircuitSettingsForHashMapping> circuitSettings)
    {
        _circuitSettings = circuitSettings.Value;
    }

    /// <summary>
    /// Verifies the proof of the user.
    /// </summary>
    /// <param name="input">The request to verify the proof of the user.</param>
    /// <returns>True if the proof is verified, otherwise false.</returns>
    public async Task<bool> VerifyAsync(VerifyInputForHashMapping input)
    {
        // TODO: Validate input
        var vk = GetVerifyingKey();

        var sha25IdHash = input.Sha256IdHash.HexStringToByteArray().Select(b => b.ToString()).ToList();

        var publicInputs = new List<List<string>>
        {
            new List<string>() { input.PoseidonIdHash }, sha25IdHash
        }.SelectMany(n => n).ToList();

        var proof = JsonConvert.DeserializeObject<InternalRapidSnarkProofRepr>(input.Proof);
        if (proof == null) throw new VerifyingException("Invalid proof format");

        return Verifier.VerifyBn254(vk, publicInputs, new RapidSnarkProof
        {
            PiA = proof.PiA,
            PiB = proof.PiB,
            PiC = proof.PiC,
        });
    }
}

internal class InternalRapidSnarkProofRepr
{
    [JsonProperty("pi_a")] public List<string> PiA { get; set; }
    [JsonProperty("pi_b")] public List<List<string>> PiB { get; set; }
    [JsonProperty("pi_c")] public List<string> PiC { get; set; }
    [JsonProperty("protocol")] public string Protocol { get; set; }
}