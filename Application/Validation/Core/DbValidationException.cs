namespace ImplementadorCUAD.Application.Validation.Core;

public sealed class DbValidationException : Exception
{
    public DbValidationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}


