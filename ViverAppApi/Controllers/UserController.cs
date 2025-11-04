using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Dtos;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppApi.Helpers;
using ViverApp.Shared.Context;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public UserController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(bool getBlocked = false, bool getRejected = false, bool getPendingApproval = false, int usertype = 0)
        {
            bool filtringByUserType = usertype != 0;

            var users = await _context.Users
                .Where(u => u.Status != (int)UserStatus.PendingEmail)
                .Where(u => getBlocked || ((u.Status == (int)UserStatus.Blocked) == getBlocked))
                .Where(u => getRejected || ((u.Status == (int)UserStatus.Rejected) == getRejected))
                .Where(u => getPendingApproval || ((u.Status == (int)UserStatus.PendingApproval) == getPendingApproval))
                .Where(u => !filtringByUserType || u.Usertype == usertype)
                .Where(u => u.Usertype != (int)UserType.Admin)
                .ToListAsync();

            var userResponseDtos = users.Select(u => new UserDto(u));
            return Ok(userResponseDtos);
        }

        [HttpGet("doctors")]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors(bool getBlocked = false, bool getRejected = false, bool getPendingApproval = false)
        {
            var doctorsdto = await _context.Users
                .Where(u => u.Usertype == (int)UserType.Doctor)
                .Where(u => u.Status != (int)UserStatus.PendingEmail)
                .Where(u => getBlocked || ((u.Status == (int)UserStatus.Blocked) == getBlocked))
                .Where(u => getRejected || ((u.Status == (int)UserStatus.Rejected) == getRejected))
                .Where(u => getPendingApproval || ((u.Status == (int)UserStatus.PendingApproval) == getPendingApproval))
                .Where(u => u.Iduser > 1)
                .Join(
                    _context.DoctorProps,
                    u => u.Iduser,
                    dp => dp.Iddoctor,
                    (u, dp) => new DoctorDto(new UserDto(u), dp)
                )
                .ToListAsync();

            return Ok(doctorsdto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            UserDto userDto = new(user);
            return Ok(userDto);
        }

        [HttpGet("doctor/{id}")]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
                return NotFound();

            UserDto userDto = new(user);

            var dp = await _context.DoctorProps.FirstOrDefaultAsync(dp => dp.Iddoctor == id);
            if (dp is null)
                return NotFound();

            DoctorDto docDto = new(userDto, dp);
            return Ok(docDto);
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            bool existsEmail = await _context.Users.AnyAsync(u => u.Email == user.Email);
            if (existsEmail)
                return BadRequest(new { message = "O email informado já está cadastrado" });

            var existsCpf = await _context.Users.FirstOrDefaultAsync(u => u.Cpf == user.Cpf && u.Usertype == user.Usertype);
            if (existsCpf is not null)
            {
                string status = (UserStatus)existsCpf.Status switch
                {
                    UserStatus.Active => "Ativo",
                    UserStatus.PendingEmail => "Email pendente de confirmação",
                    UserStatus.PendingApproval => "Aprovação pendente por um administrador",
                    UserStatus.Rejected => "Cadastro Rejeitado",
                    UserStatus.Blocked => "Cadastro Bloqueado",
                    _ => "Situação Desconhecida"
                };

                return BadRequest($"Já existe um cadastro para o CPF {existsCpf.Cpf} e o tipo de usuário escolhido.\nEmail registrado: {existsCpf.Email}." +
                    $"\nSTATUS do cadastro: {status}");
            }
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            user.Password = PasswordHelper.EncryptPassword(user.Iduser, user.Password ?? string.Empty);

            _context.Entry(user).Property(u => u.Password).IsModified = true;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Iduser }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDto user)
        {
            if (id != user.IdUser)
                return BadRequest();

            bool existsEmail = await _context.Users.AnyAsync(u => u.Email == user.Email && u.Iduser != user.IdUser);
            if (existsEmail)
                return BadRequest("O email informado já está cadastrado.");

            bool existsCpf = await _context.Users.AnyAsync(u => u.Cpf == user.Cpf && u.Iduser != user.IdUser && u.Usertype == user.Usertype);
            if (existsCpf)
                return BadRequest("Já existe o CPF informado para outro usuário com o mesmo tipo de acesso que o seu.");

            var currentUser = _context.Users.FirstOrDefault(u => u.Iduser == id);
            if (currentUser is null)
                return NotFound();

            if ((currentUser.Status == (int)UserStatus.PendingApproval || currentUser.Status == (int)UserStatus.Rejected)
                && currentUser.Status != user.Status)
            {
                EmailSender.Context = _context;

                if (user.Status == (int)UserStatus.Active)
                {
                    await EmailSender.SendEmailAsync(currentUser.Email!, "Cadastro Aprovado",
                        @$"<html>
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
                                color: #27ae60;
                                text-align: center;
                              }}
                              .status-box {{
                                margin: 25px 0;
                                padding: 18px;
                                text-align: center;
                                font-size: 20px;
                                font-weight: bold;
                                color: #ffffff;
                                background: #27ae60;
                                border-radius: 8px;
                                letter-spacing: 2px;
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
                              <h2>Cadastro Aprovado 🎉</h2>
                              <p>Olá, {currentUser.Name}! Temos uma ótima notícia pra você 😊</p>

                              <div class='status-box'>Seu cadastro foi aprovado!</div>

                              <p>
                                Seja bem-vindo(a) ao <b>Viver App</b>! A partir de agora, você já pode acessar todos os recursos disponíveis e aproveitar ao máximo nossa plataforma.
                              </p>

                              <p>
                                Ficamos muito felizes em tê-lo(a) conosco! 💙
                              </p>

                              <div class='footer'>
                                &copy; {DateTime.Now.Year} Viver App — Todos os direitos reservados
                              </div>
                            </div>
                          </body>
                        </html>
                        ");
                }
                else if (user.Status == (int)UserStatus.Rejected)
                {
                    await EmailSender.SendEmailAsync(currentUser.Email!, "Atualização do Cadastro",
                        @$"<html>
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
                                color: #e74c3c;
                                text-align: center;
                              }}
                              .status-box {{
                                margin: 25px 0;
                                padding: 18px;
                                text-align: center;
                                font-size: 20px;
                                font-weight: bold;
                                color: #ffffff;
                                background: #e74c3c;
                                border-radius: 8px;
                                letter-spacing: 2px;
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
                              <h2>Cadastro Não Aprovado 😔</h2>
                              <p>Olá {currentUser.Name}, tudo bem?</p>

                              <div class='status-box'>Seu cadastro não foi aprovado</div>

                              <p>
                                Após analisarmos suas informações, infelizmente seu cadastro não pôde ser aprovado neste momento.
                              </p>

                              <p>
                                Mas não desanime! Você pode revisar seus dados e tentar novamente a qualquer hora.  
                                Nossa equipe está sempre à disposição para ajudar você nesse processo 💬
                              </p>

                              <p>
                                Se tiver dúvidas, é só responder este e-mail ou entrar em contato com o suporte do <b>Viver App</b>.
                              </p>

                              <div class='footer'>
                                &copy; {DateTime.Now.Year} Viver App — Todos os direitos reservados
                              </div>
                            </div>
                          </body>
                        </html>
                        ");
                }
            }

            currentUser.Email = user.Email;
            currentUser.Cpf = user.Cpf;
            currentUser.Fone = user.Fone;
            currentUser.Adress = user.Adress;
            currentUser.Neighborhood = user.Neighborhood;
            currentUser.Number = user.Number;
            currentUser.City = user.City;
            currentUser.State = user.State;
            currentUser.Postalcode = user.Postalcode;
            currentUser.Complement = user.Complement;
            currentUser.Name = user.Name;
            currentUser.Status = user.Status;
            currentUser.Birthdate = user.BirthDate;
            currentUser.Notifyemail = user.NotifyEmail ?? 0;
            currentUser.Notifypush = user.Notifypush ?? 0;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.Usertype == (int)UserType.Admin)
                return BadRequest("NÃO É PERMITIDO EXCLUIR UM ADMINISTRADOR");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Iduser == id);
        }
    }
}
