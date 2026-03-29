using System.Net;
using AlugueldeCarros.Exceptions;
using FluentAssertions;

namespace AlugueldeCarros.Tests.Unit.Exceptions;

public class CustomExceptionTests
{
    [Fact]
    public void ValidationException_ShouldUseBadRequestStatusCode()
    {
        var exception = new ValidationException("validation failed");

        exception.Message.Should().Be("validation failed");
        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
}