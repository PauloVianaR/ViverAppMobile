using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;

namespace ViverAppApi.Helpers
{
    public class AuthMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, ViverappmobileContext db, IConfiguration config)
        {
            var path = context.Request.Path.Value;
            List<string> pathWhiteList =
            [
                "/Auth/getAppMode",
                "/Auth/login",
                "/Auth/refresh", 
                "/Auth/register", 
                "/Auth/clearTokens",
                "/Auth/resetPassword",
                "/Auth/getUserTypeByEmail",
                "/Auth/confirmemail",
                "/Auth/sendConfirmationEmail",
                "/swagger", 
                "/Notification",
                "/Pagbank"
            ];

            if (path is not null && pathWhiteList.FirstOrDefault(p => path.Contains(p, StringComparison.InvariantCultureIgnoreCase)) is not null)
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token ausente");
                return;
            }

            var token = authHeader["Bearer ".Length..].Trim();

            try
            {
                var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (!int.TryParse(userIdClaim, out int id) || string.IsNullOrEmpty(role))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token inválido");
                    return;
                }

                var cfgMaster = await db.Configs.FirstOrDefaultAsync(c => c.Idconfig == 13);
                if (cfgMaster is null || cfgMaster.Value != 1)
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await context.Response.WriteAsync("Aplicativo offline");
                    return;
                }

                if (role == "Admin")
                {
                    await _next(context);
                    return;
                }

                var cfg = await db.Configs.FirstOrDefaultAsync(c => c.Idconfig == 1);
                if (cfg is null || cfg.Value != 1)
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await context.Response.WriteAsync("Aplicativo em manutenção");
                    return;
                }

                if (role == "User")
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.Iduser == id) ?? throw new Exception("Usuário não encontrado");

                    _ = await db.UserTokens.FirstOrDefaultAsync(t => t.Iduser == user.Iduser && t.Revoked == 0)
                        ?? throw new Exception("Senha expirada. Por favor, faça o login novamente");

                    if (user.Status != (int)UserStatus.Active)
                    {
                        string msg = (UserStatus)user.Status switch
                        {
                            UserStatus.PendingEmail => "Email pendente de confirmação",
                            UserStatus.PendingApproval => "Aprovação pendente por um administrador",
                            UserStatus.Rejected => "Cadastro Rejeitado",
                            UserStatus.Blocked => "Cadastro Bloqueado",
                            _ => "Situação Desconhecida"
                        };

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync(msg);
                        var userTokenList = db.UserTokens.Where(u => u.Iduser == id).ForEachAsync(u => db.UserTokens.Remove(u));
                        await db.SaveChangesAsync();

                        return;
                    }

                    context.Items["User"] = user;
                }
            }
            catch
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token inválido");
                return;
            }

            await _next(context);
        }
    }
}