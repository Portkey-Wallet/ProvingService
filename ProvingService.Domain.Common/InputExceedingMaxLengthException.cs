using System;

namespace ProvingService.Domain.HashMapping;

public class InputExceedingMaxLengthException : Exception
{
    public InputExceedingMaxLengthException(string message) : base(message)
    {
    }
}