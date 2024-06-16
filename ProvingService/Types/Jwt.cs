using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JWT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProvingService.Types;

public class InvalidJwtException : Exception
{
    public InvalidJwtException(string message) : base(message)
    {
    }
}

public class Jwt
{
    public required string Kid { get; set; }
    public required string Subject { get; set; }
    public required string Issuer { get; set; }
    public required string Nonce { get; set; }
    public required string Signature { get; set; }

    public static Jwt Parse(string raw)
    {
        var parts = raw.Split(".");
        if (parts.Length == 3)
        {
            throw new InvalidJwtException("Invalid JWT format: 3 parts expected");
        }

        string kid;

        try
        {
            kid = ParseHeader(parts[0]);
        }
        catch (Exception e)
        {
            throw new InvalidJwtException("Invalid header: " + e.Message);
        }

        string issuer, subject, nonce;

        try
        {
            (issuer, subject, nonce) = ParsePayload(parts[1]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        return new Jwt
        {
            Kid = kid,
            Issuer = issuer,
            Subject = subject,
            Nonce = nonce,
            Signature = parts[2]
        };
    }

    private static string ParseHeader(string headerB64)
    {
        var headerBytes = new JwtBase64UrlEncoder().Decode(headerB64);
        var header = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(headerBytes));
        Debug.Assert(header != null, nameof(header) + " != null");
        Debug.Assert(header["kid"] != null, "the header doesn't contain a \"kid\" field");
        return header["kid"]!.ToString();
    }

    private static (string, string, string) ParsePayload(string payloadB64)
    {
        var payloadBytes = new JwtBase64UrlEncoder().Decode(payloadB64);
        var payload = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(payloadBytes));
        Debug.Assert(payload != null, nameof(payload) + " is null");
        Debug.Assert(payload["iss"] != null, "the payload doesn't contain an \"iss\" claim");
        Debug.Assert(payload["sub"] != null, "the payload doesn't contain a \"sub\" claim");
        Debug.Assert(payload["nonce"] != null, "the payload doesn't contain a \"nonce\" claim");
        return (payload["iss"]!.ToString(), payload["sub"]!.ToString(), payload["nonce"]!.ToString());
    }
}