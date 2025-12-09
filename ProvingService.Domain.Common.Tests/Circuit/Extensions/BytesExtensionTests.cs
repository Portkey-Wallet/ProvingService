namespace ProvingService.Domain.Common.Circuit.Extensions;

public class BytesExtensionTests
{
    [Fact]
    public void Test_ShiftArrayRight_OutOfBound()
    {
        var array = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var shiftBits = 41;
        Assert.Throws<ArgumentOutOfRangeException>(() => BytesExtension.ShiftArrayRight(array, shiftBits));
    }

    [Fact]
    public void Test_ShiftBytesRight_OutOfBound()
    {
        var array = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var shiftBytes = 6;
        Assert.Throws<ArgumentOutOfRangeException>(() => BytesExtension.ShiftBytesRight(array, shiftBytes));
    }

    [Fact]
    public void Test_Mask_OutOfBound()
    {
        var array = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var maskBits = 41;
        Assert.Throws<ArgumentOutOfRangeException>(() => BytesExtension.Mask(array, maskBits));
    }
}