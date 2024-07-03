using System;
using System.Collections.Generic;

namespace ProvingService.Domain.Common.Circuit.Extensions
{
    public static class BytesExtension
    {
        private static void ShiftArrayRight(byte[] array, int shiftBits)
        {
            var shiftBytes = shiftBits / 8;
            var shiftBitsMod8 = shiftBits % 8;
            if (shiftBitsMod8 == 0)
            {
                ShiftBytesRight(array, shiftBytes);
                return;
            }

            var shiftBitsMod8Complement = (8 - shiftBitsMod8) % 8;
            var length = array.Length;
            var remainingBytes = length - shiftBytes;

            var end = length - 1;
            for (var nFromEndNew = 0; nFromEndNew < remainingBytes; nFromEndNew++) // nFromEnd in the new array
            {
                var nFromEndOld = nFromEndNew + shiftBytes; // nFromEnd in the old array
                var indexNew = end - nFromEndNew;
                var indexOld = end - nFromEndOld;
                var curByte = array[indexOld];
                var previousByte = indexOld > 0 ? array[indexOld - 1] : 0;
                var rightBitsOfPreviousByte = (byte)(previousByte << shiftBitsMod8Complement);
                var leftBitsOfCurByte = (byte)(curByte >> shiftBitsMod8);
                array[indexNew] = (byte)(rightBitsOfPreviousByte | leftBitsOfCurByte);
            }

            for (var i = 0; i < shiftBytes; i++)
            {
                array[i] = 0;
            }
        }

        private static void ShiftBytesRight(byte[] array, int shiftBytes)
        {
            var length = array.Length;
            var remainingBytes = length - shiftBytes;

            var end = length - 1;
            for (var nFromEndNew = 0; nFromEndNew < remainingBytes; nFromEndNew++) // nFromEnd in the new array
            {
                var nFromEndOld = nFromEndNew + shiftBytes; // nFromEnd in the old array
                var indexNew = end - nFromEndNew;
                var indexOld = end - nFromEndOld;
                array[indexNew] = array[indexOld];
            }

            for (var i = 0; i < shiftBytes; i++)
            {
                array[i] = 0;
            }
        }

        private static byte[] Mask(byte[] array, int maskBits)
        {
            var maskBytes = (maskBits - 1) / 8 + 1; // ceil(maskBits / 8)
            var maskBitsOfPartialByte = maskBits % 8;
            var length = array.Length;
            var masked = new byte[maskBytes];
            var lastIndexOfMasked = maskBytes - 1;

            var lastIndex = length - 1;
            for (var i = 0; i < maskBytes; i++)
            {
                masked[lastIndexOfMasked - i] = array[lastIndex - i];
            }

            byte mask = 0;

            for (var i = 0; i < maskBitsOfPartialByte; i++)
            {
                mask |= (byte)(1 << i);
            }

            if (maskBitsOfPartialByte > 0)
                masked[0] = (byte)(masked[0] & mask);
            return masked;
        }


        public static IList<string> ToChunked(this byte[] bytes, int bytesPerChunk, int numOfChunks)
        {
            var chunks = new List<string>();
            for (var i = 0; i < numOfChunks; i++)
            {
                var chunk = Mask(bytes, bytesPerChunk);
                var chunkString = BitConverter.ToString(chunk).Replace("-", "");
                chunks.Add(chunkString);
                ShiftArrayRight(bytes, bytesPerChunk);
            }

            return chunks;
        }
    }
}