using System.Net;

namespace AlugueldeCarros.Exceptions;

public class CustomException : Exception
{
    public int StatusCode { get; }

    public CustomException(string message, int statusCode = (int)HttpStatusCode.BadRequest) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class ValidationException : CustomException
{
    public ValidationException(string message) : base(message, (int)HttpStatusCode.BadRequest) { }
}