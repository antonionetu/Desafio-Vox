using System.IdentityModel.Tokens.Jwt;

namespace Vox.API.Middlewares
{
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                context.Items["Token"] = token;

                try
                {
                    var jwtHandler = new JwtSecurityTokenHandler();

                    if (!jwtHandler.CanReadToken(token))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Error = "Token inválido."
                        });
                        return;
                    }

                    var jwtToken = jwtHandler.ReadJwtToken(token);

                    if (jwtToken.ValidTo < DateTime.UtcNow)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Error = "Token expirado."
                        });
                        return;
                    }
                }
                catch (Exception)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        Error = "Token inválido."
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
