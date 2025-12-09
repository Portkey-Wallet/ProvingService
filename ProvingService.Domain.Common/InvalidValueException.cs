using System;

namespace ProvingService.Domain.Common;

public class InvalidValueException : Exception
{
    public InvalidValueException(string message) : base(message)
    {
    }
}