using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ViverApp.Shared.Dtos;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppApi.Helpers;
using ViverApp.Shared.Context;

[Route("api/v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ViverappmobileContext _context;
    private readonly IConfiguration _config;

    public AuthController(ViverappmobileContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        EmailSender.Context = context;
    }

    [HttpGet("getAppMode")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> GetAppMode()
    {
        var config = await _context.Configs.FirstOrDefaultAsync(c => c.Idconfig == (int)ConfigType.ProductionMode);
        if ((config?.Value ?? (sbyte)0) == (sbyte)1)
            return Ok(true);

        return Ok(false);
    }

    [HttpGet("getUserTypeByEmail")]
    [AllowAnonymous]
    public async Task<ActionResult<int>> GetUserTypeByEmail(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is not null)
        {
            return Ok(user.Usertype);
        }

        return NotFound("Ops...Não conseguimos identificar seu usuário.\n\nVerifique se o email está correto e tente novamente");
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var user = request.User;
            if (user.Password is null)
                return BadRequest("A senha não pode ser vazia");

            var cfg = await _context.Configs.FirstOrDefaultAsync(c => c.Idconfig == (int)ConfigType.AppOnline);
            if (cfg is null || cfg.Value != 1 && user.Usertype != (int)UserType.Admin)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { message = "Aplicativo Offline" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("Email já cadastrado.");

            if (await _context.Users.AnyAsync(u => u.Cpf == user.Cpf && u.Usertype == user.Usertype))
                return BadRequest("Já existe um usuário cadastrado com este CPF para o tipo informado");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Password = PasswordHelper.EncryptPassword(user.Iduser, user.Password);
            await _context.SaveChangesAsync();

            var doctorPropCreate = request.DoctorProp;
            if (doctorPropCreate is not null && user.Usertype == (int)UserType.Doctor)
            {
                doctorPropCreate.Iddoctor = user.Iduser;
                _context.DoctorProps.Add(new DoctorProp(doctorPropCreate));
                await _context.SaveChangesAsync();
            }

            var accessToken = GenerateAccessToken(user, "User");
            var refreshToken = GenerateRefreshToken();

            if (user.Usertype == (int)UserType.Patient)
            {
                var refreshEntity = new UserToken
                {
                    Iduser = user.Iduser,
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    Revoked = (sbyte)0,
                    IduserNavigation = user
                };
                _context.UserTokens.Add(refreshEntity);
                await _context.SaveChangesAsync();
            }

            var dto = new UserDto(user);
            return Ok(new { access_token = accessToken, refresh_token = refreshToken, user = dto });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPost("sendConfirmationEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] string userEmail)
    {
        try
        {
            Random hashrand = new();
            int hashcode = hashrand.Next(0, 999999);

            string hash = $"se você está lendo isso, parabén$, você é um ótimo HACKER ;) [{DateTime.UtcNow.ToLongDateString()}] <3 || Código: {hashcode.GetHashCode()}";
            int seed = hash.GetHashCode();
            Random x = new(seed);
            int confirmationCode = x.Next(1000, 10000);

            await EmailSender.SendEmailConfirmationAsync(userEmail, "Confirmação de Email",
                $@"<html>
                  <head>
                    <style>
                      body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f6f8;
                        margin: 0;
                        padding: 0;
                      }}
                      .container {{
                        max-width: 600px;
                        margin: 30px auto;
                        background: #ffffff;
                        padding: 25px 35px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                      }}
                      h2 {{
                        color: #2c3e50;
                        text-align: center;
                      }}
                      .code-box {{
                        margin: 25px 0;
                        padding: 18px;
                        text-align: center;
                        font-size: 26px;
                        font-weight: bold;
                        color: #ffffff;
                        background: #3498db;
                        border-radius: 8px;
                        letter-spacing: 6px;
                      }}
                      p {{
                        font-size: 14px;
                        color: #333;
                        line-height: 1.6;
                      }}
                      .footer {{
                        margin-top: 30px;
                        font-size: 12px;
                        color: #777;
                        text-align: center;
                      }}
                    </style>
                  </head>
                  <body>
                    <div class='container'>
                      <h2>Confirmação de Código</h2>
                      <p>Olá! Para concluir a verificação do seu e-mail, utilize o código abaixo:</p>

                      <div class='code-box'>{confirmationCode}</div>

                      <p>
                        Insira este código no aplicativo <b>Viver</b> para confirmar sua identidade e prosseguir com o processo.
                      </p>
                      <p>
                        <b>Obs:</b> este código expira em <b>15 minutos</b>. Caso o tempo expire, será necessário solicitar um novo.
                      </p>

                      <div class='footer'>
                        &copy; {DateTime.Now.Year} Viver App — Todos os direitos reservados
                      </div>
                    </div>
                  </body>
                </html>" 
            , confirmationCode);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPost("confirmemail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] EmailValidationRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("O email de confirmação não pode ser vazio!");

            if (request.ConfirmationCode < 1000 || request.ConfirmationCode > 9999)
                return BadRequest("Código de confirmação inválido.");

            var existingConfirmEmail = await _context.EmailConfirmations
                .OrderByDescending(e => e.Idemailconfirmation)
                .FirstOrDefaultAsync(e => e.IdemailNavigation.Receiver == request.Email
                && e.Confirmationcode == request.ConfirmationCode);

            if (existingConfirmEmail is null)
                return BadRequest("O Código de confirmação informado é inválido");

            if (existingConfirmEmail.Expiresat < DateTime.Now)
                return BadRequest("Tempo de confirmação limite alcançado. Reenvie o código e resete o tempo de confirmação");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser is null)
                return NotFound("Usuário não identificado para a aprovação");

            existingUser.Status = existingUser.Usertype == (int)UserType.Patient
                ? (int)UserStatus.Active
                : (int)UserStatus.PendingApproval;
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (request.Email == "admin@email.com")
                request.UserType = (int)UserType.Admin;

            var cfg = await _context.Configs.FirstOrDefaultAsync(c => c.Idconfig == (int)ConfigType.AppOnline);
            if (cfg is null || cfg.Value != 1 && request.UserType != 1)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { message = "Aplicativo em manutenção" });
            }

            var cfgMaster = await _context.Configs.FirstOrDefaultAsync(c => c.Idconfig == (int)ConfigType.AppOnlineMaster);
            if (cfgMaster is null || cfgMaster.Value != 1)
            {
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new { message = "Aplicativo Offline" });
            }

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
                return NotFound("Ops...Não conseguimos identificar seu usuário.\n\nVerifique se o email está correto e tente novamente");

            string decryptedPassword = PasswordHelper.DecryptPasswordFromBase64(user.Password ?? string.Empty);
            if (!decryptedPassword.Contains(':'))
                return BadRequest("A senha informada é inválida.");

            var password = (decryptedPassword?.Split(':')?.GetValue(1)?.ToString()) ?? "";
            if (password != request.Password)
                return BadRequest("Senha incorreta");

            string role = request.UserType == (int)UserType.Admin ? "Admin" : "User";

            var accessToken = GenerateAccessToken(user, role);
            var refreshToken = GenerateRefreshToken();

            await _context.UserTokens.Where(t => t.Iduser == user.Iduser)
                .ForEachAsync(t => t.Revoked = 1);

            var refreshEntity = new UserToken
            {
                Iduser = user.Iduser,
                Token = refreshToken,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMonths(1),
                Revoked = (sbyte)0,
                IduserNavigation = user
            };
            _context.UserTokens.Add(refreshEntity);
            user.Devicetoken = request.Devicetoken;

            var dto = new UserDto(user);

            await _context.SaveChangesAsync();

            return Ok(new { access_token = accessToken, refresh_token = refreshToken, user = dto });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken) || request.UserType == 0)
                return BadRequest("Refresh token inválido ou tipo de usuário não informado");

            var stored = await _context.UserTokens
                    .Include(t => t.IduserNavigation)
                    .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.Revoked == 0);

            if (stored is null || stored.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Refresh token inválido ou expirado");

            var user = stored.IduserNavigation;
            var newAccess = GenerateAccessToken(user, request.UserType == 1 ? "Admin" : "User");
            var newRefresh = GenerateRefreshToken();

            stored.Revoked = 1;
            _context.UserTokens.Update(stored);

            _context.UserTokens.Add(new UserToken
            {
                Iduser = user.Iduser,
                Token = newRefresh,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMonths(1),
                Revoked = 0,
                IduserNavigation = user
            });
            await _context.SaveChangesAsync();

            var dto = new UserDto(user);

            return Ok(new { access_token = newAccess, refresh_token = newRefresh, user = dto });
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private string GenerateAccessToken(User user, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "9uCQCTAESp9uBpAoBmo5MD3uW82YAwTD");

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, user.Iduser.ToString()),
            new (ClaimTypes.Name, user.Name ?? string.Empty),
            new (ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));


    [HttpPatch("changePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = request.Id;
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (request.Id == 0)
                return BadRequest("Não foi possível encontrar o usuário para a alteração da senha");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Iduser == userId);
            if (user is null)
                return NotFound("Usuário não encontrado");

            string decryptedPassword = PasswordHelper.DecryptPasswordFromBase64(user.Password ?? string.Empty);
            var password = (decryptedPassword?.Split(':')?.GetValue(1)?.ToString()) ?? "";

            if (password != request.OldPassword)
                return BadRequest("Senha atual incorreta");
            if (string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("A nova senha não pode ser vazia");
            if (password.Equals(request.NewPassword))
                return BadRequest("A nova senha não pode ser igual à atual");

            user.Password = PasswordHelper.EncryptPassword(user.Iduser, request.NewPassword);
            await _context.SaveChangesAsync();

            var usertokens = await _context.UserTokens.Where(t => t.Iduser == user.Iduser && t.Revoked != (sbyte)1).ToListAsync();
            usertokens.ForEach(t => t.Revoked = (sbyte)1);

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpDelete("clearTokens/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteUserTokens(int id, [FromQuery] bool clearDeviceToken)
    {

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Iduser == id);
        if (user is not null)
        {
            var tokens = await _context.UserTokens.Where(t => t.Iduser == user.Iduser).ToListAsync();
            _context.UserTokens.RemoveRange(tokens);

            if(clearDeviceToken)
                user.Devicetoken = null;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("resetPassword")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> ResetUserPass([FromBody] string email)
    {
        try
        {
            email = email.ToLower().Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return BadRequest("Não existe um usuário com este email para que a senha possa ser redefinida.");

            string userEmail = user.Email ?? string.Empty;

            string tempPassword = PasswordHelper.GenerateTemporaryPassword();

            user.Password = PasswordHelper.EncryptPassword(user.Iduser, tempPassword);
            await _context.SaveChangesAsync();

            var usertokens = await _context.UserTokens.Where(t => t.Iduser == user.Iduser && t.Revoked != (sbyte)1).ToListAsync();
            usertokens.ForEach(t => t.Revoked = (sbyte)1);

            await _context.SaveChangesAsync();

            await EmailSender.SendEmailAsync(userEmail, "Redefinição de Senha",
                $@"
                <html>
                  <head>
                    <style>
                      body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f6f8;
                        margin: 0;
                        padding: 0;
                      }}
                      .container {{
                        max-width: 600px;
                        margin: 30px auto;
                        background: #ffffff;
                        padding: 20px 30px;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
                      }}
                      h2 {{
                        color: #2c3e50;
                        text-align: center;
                      }}
                      .password-box {{
                        margin: 20px 0;
                        padding: 15px;
                        text-align: center;
                        font-size: 20px;
                        font-weight: bold;
                        color: #ffffff;
                        background: #3498db;
                        border-radius: 6px;
                        letter-spacing: 2px;
                      }}
                      p {{
                        font-size: 14px;
                        color: #333;
                        line-height: 1.6;
                      }}
                      .footer {{
                        margin-top: 25px;
                        font-size: 12px;
                        color: #777;
                        text-align: center;
                      }}
                    </style>
                  </head>
                  <body>
                    <div class='container'>
                      <h2>Redefinição de Senha</h2>
                      <p>Olá! Sua nova senha temporária é:</p>
                      <div class='password-box'>{tempPassword}</div>
                      <p>
                        Use-a para entrar no aplicativo <b>Viver</b> e, uma vez logado,
                        altere sua senha na aba <b>Perfil</b>.
                      </p>
                      <p><b>Obs:</b> esta senha expira em <b>24 horas</b>. Caso não seja alterada nesse período,
                      será necessário solicitar novamente uma nova senha.</p>
                      <div class='footer'>
                        &copy; {DateTime.Now.Year} Viver App — Todos os direitos reservados
                      </div>
                    </div>
                  </body>
                </html>
                ");

            return Ok("Uma nova senha temporária foi enviada para o seu email.\nVerifique na caixa de entrada normal ou na caixa de SPAM.\n\nObs: esta nova senha tem validade de 24hrs e, caso não seja alterada neste período, será necessário solicitar novamente uma nova senha.");
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
