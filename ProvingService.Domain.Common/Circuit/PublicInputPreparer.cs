using System.Collections.Generic;
using System.Linq;
using System.Text;
using JWT;
using ProvingService.Domain.Common.Circuit.Extensions;

namespace ProvingService.Domain.Common.Circuit;

public static class PublicInputPreparer
{
    public static List<string> Prepare(string nonce, string pubkey, byte[] salt)
    {
        var pubkeyChunks = new JwtBase64UrlEncoder().Decode(pubkey)
            .ToChunked(Constants.CircomBigIntN, Constants.CiromBigIntK)
            .Select(HexStringExtension.HexToBigInt).ToList();
        var nonceInInts = Encoding.UTF8.GetBytes(nonce).Select(b => b.ToString()).ToList();
        var saltInInts = salt.Select(b => b.ToString()).ToList();
        return new List<List<string>>
        {
            nonceInInts, pubkeyChunks, saltInInts
        }.SelectMany(n => n).ToList();
    }
}