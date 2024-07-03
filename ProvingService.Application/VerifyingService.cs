using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Groth16.Net;
using JWT;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProvingService.Application.Contracts;
using ProvingService.Domain.Common.Circuit.Extensions;
using ProvingService.Domain.Common.Extensions;

namespace ProvingService.Application;

public class VerifyingException : Exception
{
    public VerifyingException(string message) : base(message)
    {
    }
}

/// <summary>
/// For convenience, the VerifyingService class is used to verify the proof of the user.
/// </summary>
public class VerifyingService : IVerifyingService
{
    private const int CircomBigIntN = 121;
    private const int CiromBigIntK = 17;
    private CircuitSettings _circuitSettings;
    private string? _verifyingKey;
    private IJwksService _jwksService;
    private readonly IIdentifierHashService _identifierHashService;

    public string GetVerifyingKey()
    {
        if (_verifyingKey != null) return _verifyingKey;
        using var prover = Prover.Create(_circuitSettings.WasmPath, _circuitSettings.R1csPath,
            _circuitSettings.ZkeyPath);
        _verifyingKey = prover.ExportVerifyingKeyBn254();

        return _verifyingKey.TrimEnd('\u0000');
    }

    public VerifyingService(IOptionsSnapshot<CircuitSettings> circuitSettings, IJwksService jwksService,
        IIdentifierHashService identifierHashService)
    {
        _circuitSettings = circuitSettings.Value;
        _jwksService = jwksService;
        _identifierHashService = identifierHashService;
    }

    /// <summary>
    /// Verifies the proof of the user.
    /// </summary>
    /// <param name="request">The request to verify the proof of the user.</param>
    /// <returns>True if the proof is verified, otherwise false.</returns>
    public async Task<bool> VerifyAsync(VerifyInput request)
    {
        var vk = GetVerifyingKey();

        var pubkey = await _jwksService.GetKeyAsync(request.Kid);
        var pubkeyChunks = new JwtBase64UrlEncoder().Decode(pubkey)
            .ToChunked(CircomBigIntN, CiromBigIntK)
            .Select(HexToBigInt).ToList();
        var proof = JsonConvert.DeserializeObject<InternalRapidSnarkProofRepr>(request.Proof);
        if (proof == null) throw new VerifyingException("Invalid proof format");

        var nonceInInts = Encoding.UTF8.GetBytes(request.Nonce).Select(b => b.ToString()).ToList();
        var saltInInts = request.Salt.HexStringToByteArray().Select(b => b.ToString()).ToList();

        var publicInputs = new List<List<string>>
        {
            _identifierHashService.ToPublicInput(request.IdentifierHash),
            nonceInInts, pubkeyChunks, saltInInts
        }.SelectMany(n => n).ToList();

        return Verifier.VerifyBn254(vk, publicInputs, new RapidSnarkProof
        {
            PiA = proof.PiA,
            PiB = proof.PiB,
            PiC = proof.PiC,
        });
    }

    private static string HexToBigInt(string hexString)
    {
        return BigInteger.Parse(hexString, NumberStyles.HexNumber).ToString();
    }
}

internal class InternalRapidSnarkProofRepr
{
    [JsonProperty("pi_a")] public List<string> PiA { get; set; }
    [JsonProperty("pi_b")] public List<List<string>> PiB { get; set; }
    [JsonProperty("pi_c")] public List<string> PiC { get; set; }
    [JsonProperty("protocol")] public string Protocol { get; set; }
}