using System.Collections.Generic;
using System.Linq;

namespace ProvingService.Domain.Common.Circuit.Extensions;

public static class PaddingExtension
{
    internal static List<string> Pad(this string str, int paddedBytesSize)
    {
        var paddedBytes = str.Select(c => ((int)c).ToString()).ToList();

        var paddingLength = paddedBytesSize - paddedBytes.Count;
        if (paddingLength > 0)
        {
            paddedBytes.AddRange(Enumerable.Repeat("0", paddingLength));
        }

        return paddedBytes;
    }
}