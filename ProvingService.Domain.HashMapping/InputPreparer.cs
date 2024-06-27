using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProvingService.Domain.HashMapping;

using CircuitInput = Dictionary<string, IList<string>>;

public static class InputPreparer
{
    public static CircuitInput Prepare(string subject, byte[] salt)
    {
        var circuitInput = new CircuitInput();
        circuitInput["sub"] = Pad(subject, 255);
        circuitInput["salt"] = Pad(salt, 16);
        circuitInput["subLen"] = new List<string>() { subject.Length.ToString() };
        circuitInput["saltLen"] = new List<string>() { salt.Length.ToString() };
        return circuitInput;
    }

    private static List<string> Pad(string str, int paddedBytesSize)
    {
        return Pad(Encoding.ASCII.GetBytes(str), paddedBytesSize);
    }

    private static List<string> Pad(byte[] bytes, int paddedBytesSize)
    {
        var list = bytes.Select((Func<byte, string>)(c => ((int)c).ToString()))
            .ToList<string>();
        var count = paddedBytesSize - list.Count;
        if (count > 0)
            list.AddRange(Enumerable.Repeat<string>("0", count));
        return list;
    }
}