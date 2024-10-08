using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using ProvingService.Domain.Common.Circuit;

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
    private CircuitSettings _circuitSettings;
    private string? _verifyingKey;
    private string? _zkeyMd5;
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

    public string GetZkeyMd5()
    {
        if (_zkeyMd5 != null) return _zkeyMd5;
        using (var md5 = System.Security.Cryptography.MD5.Create())
        using (var stream = File.OpenRead(_circuitSettings.ZkeyPath))
        {
            byte[] hashBytes = md5.ComputeHash(stream);
            _zkeyMd5 = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        return _zkeyMd5;
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
        var pubkey = await _jwksService.GetKeyAsync(request.Kid);
        var proof = JsonConvert.DeserializeObject<InternalRapidSnarkProofRepr>(request.Proof);
        if (proof == null) throw new VerifyingException("Invalid proof format");

        var publicInputs = new List<List<string>>
        {
            _identifierHashService.ToPublicInput(request.IdentifierHash),
            PublicInputPreparer.Prepare(request.Nonce, pubkey, request.Salt.DecodeHex())
        }.SelectMany(n => n).ToList();

        var vk = GetVerifyingKey();
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