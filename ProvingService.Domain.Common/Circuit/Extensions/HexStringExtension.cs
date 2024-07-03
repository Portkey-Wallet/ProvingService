using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace ProvingService.Domain.Common.Circuit.Extensions;

public static class HexStringExtension
{
    public static byte[] DecodeHex(this string hex)
    {
        var length = hex.Length;
        var byteArray = new byte[length / 2];

        for (var i = 0; i < length; i += 2)
        {
            byteArray[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return byteArray;
    }

    public static IList<string> HexToChunkedBytes(this string hexString, int bytesPerChunk, int numOfChunks)
    {
        var bytes = DecodeHex(hexString);

        return bytes.ToChunked(bytesPerChunk, numOfChunks);
    }

    internal static string HexToBigInt(this string hexString)
    {
        return new BigInteger(hexString.DecodeHex(), true, true).ToString();
    }
}