using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

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
            string? authHeader = context.Request.Headers["Api_Key"];
            string requestPath = context.Request.Path;

            if (requestPath == "api/account/register" || requestPath == "api/account/login")
            {
                await _requestDelegate(context);
                return;
            }

            if (authHeader != null && !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer"))
            {
                string[] hearder_and_token = authHeader.Split(' ');
                string header = hearder_and_token[0];
                string token = hearder_and_token[1];
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

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken securityToken);

                if (principal is not null)
                {
                    await _requestDelegate(context);
                    return;
                }
                else
                {
                    context.Response.StatusCode = 401;
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
    }
}
