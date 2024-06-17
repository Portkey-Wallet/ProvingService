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
using Portkey.JwtProof.Extensions;
using ProvingService.Controllers;
using ProvingService.Helpers;

namespace ProvingService.Services;

public class VerifyingException : Exception
{
    public VerifyingException(string message) : base(message)
    {
    }
}

public interface IVerifyingService
{
    Task<bool> VerifyAsync(VerifyRequest request);
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

    private string GetVerifyingKey()
    {
        if (_verifyingKey != null) return _verifyingKey;
        using var prover = Prover.Create(_circuitSettings.WasmPath, _circuitSettings.R1csPath,
            _circuitSettings.ZkeyPath);
        _verifyingKey = prover.ExportVerifyingKeyBn254();

        return _verifyingKey;
    }

    public VerifyingService(IOptionsSnapshot<CircuitSettings> circuitSettings, IJwksService jwksService)
    {
        _circuitSettings = circuitSettings.Value;
        _jwksService = jwksService;
    }

    /// <summary>
    /// Verifies the proof of the user.
    /// </summary>
    /// <param name="request">The request to verify the proof of the user.</param>
    /// <returns>True if the proof is verified, otherwise false.</returns>
    public async Task<bool> VerifyAsync(VerifyRequest request)
    {
        var vk = GetVerifyingKey();
        var identifierHash = request.IdentifierHash.HexStringToByteArray().Select(b => b.ToString()).ToList();
        var pubkey = await _jwksService.GetKeyAsync(request.Kid);
        var pubkeyChunks = new JwtBase64UrlEncoder().Decode(pubkey)
            .ToChunked(CircomBigIntN, CiromBigIntK)
            .Select(HexToBigInt).ToList();
        var proof = JsonConvert.DeserializeObject<RapidSnarkProofRepr>(request.Proof);
        if (proof == null) throw new VerifyingException("Invalid proof format");

        var nonceInInts = Encoding.UTF8.GetBytes(request.Nonce).Select(b => b.ToString()).ToList();
        var saltInInts = request.Salt.HexStringToByteArray().Select(b => b.ToString()).ToList();

        var publicInputs = new List<List<string>>
        {
            identifierHash, nonceInInts, pubkeyChunks, saltInInts
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

public class RapidSnarkProofRepr
{
    [JsonProperty("pi_a")] public List<string> PiA { get; set; }
    [JsonProperty("pi_b")] public List<List<string>> PiB { get; set; }
    [JsonProperty("pi_c")] public List<string> PiC { get; set; }
    [JsonProperty("protocol")] public string Protocol { get; set; }
}