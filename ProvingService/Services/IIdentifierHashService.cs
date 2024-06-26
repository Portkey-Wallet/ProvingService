using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using AElf;
using Portkey.JwtProof.Extensions;
using ProvingService.Helpers;

namespace ProvingService.Services;

public interface IIdentifierHashService
{
    string GenerateIdentifierHash(string subject, byte[] salt);
    List<string> ToPublicInput(string identifierHash);
}

public class Sha256IdentifierHashService : IIdentifierHashService
{
    public string GenerateIdentifierHash(string subject, byte[] salt)
    {
        var identifierHash = Helpers.HashHelper.GetHash(Encoding.UTF8.GetBytes(subject), salt);
        return identifierHash.Value.ToHex();
    }

    public List<string> ToPublicInput(string identifierHash)
    {
        return identifierHash.HexStringToByteArray().Select(b => b.ToString()).ToList();
    }
}

public class PoseidonIdentifierHashService : IIdentifierHashService
{
    public string GenerateIdentifierHash(string subject, byte[] salt)
    {
        const int requiredSubjectLength = 255;

        var subjectBytes = Encoding.ASCII.GetBytes(subject);

        var hashString =
            new Poseidon.Net.Poseidon().Hash(ChunksToFieldElements(subjectBytes, requiredSubjectLength, 31));

        return new Poseidon.Net.Poseidon().Hash(new List<string>()
            { hashString, ChunksToFieldElements(salt, 16, 16).First() });
    }

    private List<string> ChunksToFieldElements(byte[] bytes, int requiredLength, int chunkSize = 31)
    {
        var paddedBytes = PadSucceedingZeros(bytes, requiredLength);
        var bigEndianSubjectBytes = paddedBytes.Reverse().ToArray();

        var chunks = SplitByteArrayFromEnd(bigEndianSubjectBytes, chunkSize);
        return chunks.Select(chunk => new BigInteger(chunk, true, true).ToString()).ToList();
    }

    private static byte[] PadSucceedingZeros(byte[] original, int length)
    {
        if (original.Length >= length)
            return original;
        var result = new byte[length];
        Array.Copy(original, result, original.Length);
        return result;
    }

    public List<string> ToPublicInput(string identifierHash)
    {
        return [identifierHash];
    }

    private static byte[][] SplitByteArrayFromEnd(byte[] buffer, int chunkSize)
    {
        var numChunks = (buffer.Length + chunkSize - 1) / chunkSize;
        var chunks = new byte[numChunks][];

        if (numChunks == 1)
        {
            chunks[0] = new byte[buffer.Length];
            Array.Copy(buffer, 0, chunks[0], 0, buffer.Length);
        }

        var currentIndex = buffer.Length - chunkSize;
        var currentChunkIndex = 0;
        while (true)
        {
            var currentChunkSize = Math.Min(chunkSize, buffer.Length - currentChunkIndex * chunkSize);
            chunks[currentChunkIndex] = new byte[currentChunkSize];
            Array.Copy(buffer, currentIndex, chunks[currentChunkIndex], 0, currentChunkSize);
            if (currentIndex == 0)
            {
                break;
            }
            else
            {
                currentIndex = Math.Max(0, currentIndex - currentChunkSize);
            }

            currentChunkIndex++;
        }

        return chunks;
    }
}