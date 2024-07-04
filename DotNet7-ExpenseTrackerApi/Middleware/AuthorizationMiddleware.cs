using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DotNet7_ExpenseTrackerApi.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IConfiguration _configuration;

        public AuthorizationMiddleware(IConfiguration configuration, RequestDelegate requestDelegate)
        {
            _configuration = configuration;
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? authHeader = context.Request.Headers["Authorization"];
            string requestPath = context.Request.Path;

            if (requestPath == "/api/account/register" || requestPath == "/api/account/login")
            {
                await _requestDelegate(context);
                return;
            }

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader.Substring("Bearer ".Length).Trim();
                string privateKey = _configuration.GetSection("EncryptionKey").Value!;
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes(privateKey);

                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                try
                {
                    ClaimsPrincipal principal = tokenHandler.ValidateToken(
                        token,
                        parameters,
                        out SecurityToken securityToken
                    );

                    if (principal != null)
                    {
                        await _requestDelegate(context);
                        return;
                    }
                }
                catch (SecurityTokenException)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }

            context.Response.StatusCode = 401;
            return;
        }
    }
}
