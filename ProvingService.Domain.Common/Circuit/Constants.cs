using System.Collections.Generic;
using JWT.Builder;

namespace ProvingService.Domain.Common.Circuit;

public static class Constants
{
    internal const int CircomBigIntN = 121;
    internal const int CiromBigIntK = 17;

    internal static readonly Dictionary<ClaimName, string> RegexPatterns = new()
    {
        { ClaimName.Subject, "\"sub\"\\s*:\\s*\"[\\.\\w]+\"\\s*[,}]" },
        { ClaimName.Nonce, "\"nonce\"\\s*:\\s*\"[\\w]+\"\\s*[,}]" },
        { ClaimName.ExpirationTime, "\"exp\"\\s*:\\s*\\d+\\s*[,}]" },
    };

    internal static readonly Dictionary<string, int> StringLengths = new()
    {
        { "jwt", 1024 },
        { "sub", 264 },
        { "exp", 17 },
        { "nonce", 77 },
    };

    public static List<string> ExpectedInputFields = new()
    {
        "jwt",
        "signature",
        "pubkey",
        "salt",
        "payload_start_index",
        "sub_claim",
        "sub_claim_length",
        "sub_index_b64",
        "sub_length_b64",
        "sub_name_length",
        "sub_colon_index",
        "sub_value_index",
        "sub_value_length",
        "exp_claim",
        "exp_claim_length",
        "exp_index_b64",
        "exp_length_b64",
        "exp_name_length",
        "exp_colon_index",
        "exp_value_index",
        "exp_value_length",
        "nonce_claim",
        "nonce_claim_length",
        "nonce_index_b64",
        "nonce_length_b64",
        "nonce_name_length",
        "nonce_colon_index",
        "nonce_value_index",
        "nonce_value_length"
    };
}