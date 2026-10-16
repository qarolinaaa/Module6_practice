using System.Security.Cryptography;
using System.Text;
namespace ProductApi.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    { _next = next; _configuration = configuration; }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/product"))
        { await _next(context); return; }

        var headerName = _configuration["ApiKey:HeaderName"] ?? "X-Api-Key";
        var expected = Encoding.UTF8.GetBytes(_configuration["ApiKey:Value"] ?? "");
        if (!context.Request.Headers.TryGetValue(headerName, out var supplied) ||
            string.IsNullOrWhiteSpace(supplied) ||
            !FixedEquals(expected, Encoding.UTF8.GetBytes(supplied.ToString())))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new {
                error = "Unauthorized", message = $"A valid {headerName} header is required."
            });
            return;
        }
        await _next(context);
    }

    private static bool FixedEquals(byte[] a, byte[] b) =>
        a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
}
