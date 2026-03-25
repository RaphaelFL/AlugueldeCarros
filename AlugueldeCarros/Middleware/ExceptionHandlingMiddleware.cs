using System.Net;
using System.Text.Json;
using AlugueldeCarros.Exceptions;

namespace AlugueldeCarros.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomException ex)
        {
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, (int)HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(context, (int)HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new
        {
            error = message
        });

        return context.Response.WriteAsync(result);
    }
}