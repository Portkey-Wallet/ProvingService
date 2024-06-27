using System;

namespace ProvingService.Application;

public class ProofGenerationException : Exception
{
    public ProofGenerationException(string message) : base(message)
    {
    }
}