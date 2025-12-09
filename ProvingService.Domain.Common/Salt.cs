using System;
using ProvingService.Domain.Common.Extensions;

namespace ProvingService.Domain.Common;

public class InvalidSaltException : Exception
{
    public InvalidSaltException(string message) : base(message)
    {
    }
}

public class Salt
{
    public byte[] Value { get; set; } = null!;

    public static Salt Parse(string raw)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(raw, @"^(0x|0X)?[a-fA-F0-9]{32}$"))
            throw new InvalidSaltException(
                "Invalid salt: Salt must be a 32-character hexadecimal string (i.e. 16 bytes)");

        return new Salt()
        {
            Value = raw.Replace("0x", "").Replace("0X", "").HexStringToByteArray()
        };
    }
}