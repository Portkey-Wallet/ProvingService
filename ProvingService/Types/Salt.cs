using System;

namespace ProvingService.Types;

public class InvalidSaltException : Exception
{
    public InvalidSaltException(string message) : base(message)
    {
    }
}

public class Salt
{
    public required byte[] Value { get; set; }

    public static Salt Parse(string raw)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(raw, @"^(0x|0X)?[a-fA-F0-9]{32}$"))
            throw new InvalidSaltException(
                "Invalid salt: Salt must be a 32-character hexadecimal string (i.e. 16 bytes)");

        return new Salt()
        {
            Value = HexStringToByteArray(raw.Replace("0x", "").Replace("0X", ""))
        };
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        var length = hex.Length;
        var bytes = new byte[length / 2];
        for (var i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }
}