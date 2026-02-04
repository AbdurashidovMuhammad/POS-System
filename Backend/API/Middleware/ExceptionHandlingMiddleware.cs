using Application.DTOs;
using Application.Exceptions;

namespace API.Middleware;

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
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (DuplicateException ex)
        {
            await HandleExceptionAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ApiResult<object>.Failure(new[] { message }));
    }
}
