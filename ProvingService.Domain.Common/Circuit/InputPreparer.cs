using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JWT;
using JWT.Builder;
using ProvingService.Domain.Common.Circuit.Extensions;

namespace ProvingService.Domain.Common.Circuit;

public static class InputPreparer
{
    public static Dictionary<string, IList<string>> Prepare(string jwtB64, string pubkeyB64, byte[] salt)
    {
        var parts = jwtB64.Split('.');
        var header = parts[0];
        var payload = parts[1];
        var signature = parts[2];

        var sigBytes = new JwtBase64UrlEncoder().Decode(signature);
        var pubkeyBytes = new JwtBase64UrlEncoder().Decode(pubkeyB64);

        var rawJwt = $"{header}.{payload}";
        // 9 extra bytes for sha2 padding of one byte 0x80 and 8 bytes for the length
        var numSha2Blocks = (int)Math.Ceiling((rawJwt.Length + 9) / 64.0);
        var paddedUnsignedJwt = Sha256PreimagePadding(rawJwt)
            .Pad(Constants.StringLengths["jwt"]);

        var args = new Dictionary<string, IList<string>>()
        {
            { "padded_unsigned_jwt", paddedUnsignedJwt },
            {
                "signature",
                sigBytes.ToChunked(Constants.CircomBigIntN, Constants.CiromBigIntK)
                    .Select(s => s.HexToBigInt()).ToList()
            },
            {
                "pubkey",
                pubkeyBytes.ToChunked(Constants.CircomBigIntN, Constants.CiromBigIntK)
                    .Select(s => s.HexToBigInt()).ToList()
            },
            { "salt", salt.Select(b => b.ToString()).ToList() },
            { "payload_start_index", new List<string>() { (header.Length + 1).ToString() } },
            { "payload_len", new List<string>() { payload.Length.ToString() } },
            { "num_sha2_blocks", new List<string>() { numSha2Blocks.ToString() } },
        };
        var claimArgs = new[] { ClaimName.Subject, ClaimName.Nonce }.SelectMany(claimName =>
            jwtB64.Extract(claimName).IterArguments()
        );
        foreach (var (name, value) in claimArgs)
        {
            args.Add(name, value);
        }

        return args;
    }

    public static byte[] Sha256PreimagePadding(string message)
    {
        // Convert the message to bytes
        var messageBytes = Encoding.UTF8.GetBytes(message);

        // Step 1: Append '1' bit (0x80 in hex)
        var paddedBytes = new byte[messageBytes.Length + 1];
        Array.Copy(messageBytes, paddedBytes, messageBytes.Length);
        paddedBytes[messageBytes.Length] = 0x80;
        Console.WriteLine(paddedBytes.Length);

        // Step 2: Add zero bits
        var k = (448 - (paddedBytes.Length * 8) % 512) % 512;
        Array.Resize(ref paddedBytes, paddedBytes.Length + (k / 8));
        Console.WriteLine(paddedBytes.Length);

        // Step 3: Append original message length (64 bits)
        long originalLengthBits = messageBytes.Length * 8;
        var lengthBytes = BitConverter.GetBytes(originalLengthBits);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
        }

        Array.Resize(ref paddedBytes, paddedBytes.Length + 8);
        Array.Copy(lengthBytes, 0, paddedBytes, paddedBytes.Length - 8, 8);

        return paddedBytes;
    }

    private static ClaimInfo Extract(this string jwt, ClaimName name)
    {
        var b64Payload = FindPayloadPart(jwt);
        var jsonPayload = Decode(b64Payload); // This may fail.
        if (!Constants.RegexPatterns.TryGetValue(name, out var pattern))
            throw new JwtParsingException($"Invalid claim name {name}");

        var claim = Regex.Match(jsonPayload, pattern).Value;
        if (string.IsNullOrEmpty(claim))
        {
            throw new JwtParsingException($"Claim {name} not found in JWT");
        }

        var plainIndex = jsonPayload.IndexOf(claim, StringComparison.Ordinal);
        var b64Index = ToB64Index(plainIndex);
        // ReSharper disable once ComplexConditionExpression
        var b64End = ToB64Index(plainIndex + claim.Length + 1);
        var claimName = claim.Split(':')[0];
        var claimValue = CleanValue(claim.Split(':')[1]); // Length == 2 is guaranteed by regex
        return new ClaimInfo
        {
            Name = name,
            Claim = claim,
            B64Index = FindPayloadStartIndex(jwt) + b64Index,
            B64Length = b64End - b64Index,
            NameLength = claimName.Length,
            ColonIndex = claim.IndexOf(':'),
            ValueIndex = claim.IndexOf(claimValue, StringComparison.Ordinal),
            ValueLength = claimValue.Length,
        };

        string CleanValue(string v)
        {
            return Regex.Replace(v, "[,}\\s]*$", "").Trim().Trim('}').Trim(',');
        }

        int ToB64Index(int index)
        {
            return index / 3 * 4 + index % 3;
        }
    }

    private static int FindPayloadStartIndex(string jwt)
    {
        return jwt.IndexOf('.') + 1;
    }

    private static string FindPayloadPart(string jwt)
    {
        var parts = jwt.Split('.');
        return parts.Length switch
        {
            2 => parts[1],
            3 => parts[1],
            _ => throw new JwtParsingException("Invalid JWT format")
        };
    }

    private static string Decode(this string b64Payload)
    {
        var urlEncoder = new JwtBase64UrlEncoder();
        var decodedBytes = urlEncoder.Decode(b64Payload);
        return Encoding.UTF8.GetString(decodedBytes);
    }
}