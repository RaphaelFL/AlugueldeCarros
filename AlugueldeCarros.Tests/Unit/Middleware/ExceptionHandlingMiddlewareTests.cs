using System.Net;
using System.Text.Json;
using AlugueldeCarros.Exceptions;
using AlugueldeCarros.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace AlugueldeCarros.Tests.Unit.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_Returns401AndJsonPayload()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new UnauthorizedAccessException("Invalid credentials"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        context.Response.ContentType.Should().Be("application/json");

        var payload = await ReadResponseAsync(context);
        payload.GetProperty("error").GetString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidOperationException_Returns400AndJsonPayload()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("User already exists"));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var payload = await ReadResponseAsync(context);
        payload.GetProperty("error").GetString().Should().Be("User already exists");
    }

    [Fact]
    public async Task InvokeAsync_WithCustomException_ReturnsConfiguredStatusCode()
    {
        var context = CreateContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new CustomException("Custom error", (int)HttpStatusCode.Conflict));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);

        var payload = await ReadResponseAsync(context);
        payload.GetProperty("error").GetString().Should().Be("Custom error");
    }

    private static DefaultHttpContext CreateContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private static async Task<JsonElement> ReadResponseAsync(DefaultHttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return document.RootElement.Clone();
    }
}