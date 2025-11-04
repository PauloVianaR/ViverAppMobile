using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ViverApp.Shared.Context;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppApi.Helpers;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ScheduleAttachmentsController : ControllerBase
    {
        private readonly ViverappmobileContext _context;
        private readonly B2StorageService _b2Service;

        public ScheduleAttachmentsController(ViverappmobileContext context, B2StorageService b2Service)
        {
            _context = context;
            _b2Service = b2Service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleAttachment>>> GetScheduleAttachments()
        {
            return await _context.ScheduleAttachments.ToListAsync();
        }

        [HttpGet("getAllBySchedule/{idSchedule}")]
        public async Task<ActionResult<IEnumerable<ScheduleAttachment>>> GetAllByIdSchedule(int idSchedule)
        {
            return await _context.ScheduleAttachments
                .Where(sa => sa.Idschedule == idSchedule)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleAttachment>> GetScheduleAttachment(int id)
        {
            var scheduleAttachment = await _context.ScheduleAttachments.FindAsync(id);

            if (scheduleAttachment == null)
            {
                return NotFound();
            }

            return scheduleAttachment;
        }

        [HttpGet("download/{idScheduleAttachment}")]
        public async Task<IActionResult> Download(int idScheduleAttachment)
        {
            try
            {
                var existingAttachment = await _context.ScheduleAttachments
                    .FirstOrDefaultAsync(sa => sa.Idscheduleattachments == idScheduleAttachment);

                if (existingAttachment is null)
                    return NotFound("Não foi encontrado um arquivo para download.");

                return Ok(existingAttachment.Filepath);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ScheduleAttachment>> PostScheduleAttachment([FromForm] ScheduleAttachmentDto dto)
        {
            try
            {
                if (dto.File is null)
                    return BadRequest("Nenhum arquivo enviado.");

                using var stream = dto.File.OpenReadStream();
                var returnArray = await _b2Service.UploadFileAsync(stream, dto.File.FileName);

                string url = returnArray[0];
                string finalFileName = returnArray[1];

                if (string.IsNullOrWhiteSpace(url))
                    return BadRequest("Ops... Ocorreu uma falha ao recuperar o arquivo.");

                var newScheduleAttechment = new ScheduleAttachment()
                {
                    Idschedule = dto.Idschedule,
                    Filepath = url,
                    Filename = finalFileName,
                    Size = dto.Size
                };

                _context.ScheduleAttachments.Add(newScheduleAttechment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetScheduleAttachment), new { id = newScheduleAttechment.Idscheduleattachments }, newScheduleAttechment);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheduleAttachment(int id, ScheduleAttachment scheduleAttachment)
        {
            if (id != scheduleAttachment.Idscheduleattachments)
            {
                return BadRequest();
            }

            _context.Entry(scheduleAttachment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleAttachmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleAttachment(int id)
        {
            try
            {
                var scheduleAttachment = await _context.ScheduleAttachments.FindAsync(id);
                if (scheduleAttachment == null)
                {
                    return NotFound();
                }

                await _b2Service.DeleteFileAsync(scheduleAttachment.Filename);

                _context.ScheduleAttachments.Remove(scheduleAttachment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private bool ScheduleAttachmentExists(int id)
        {
            return _context.ScheduleAttachments.Any(e => e.Idscheduleattachments == id);
        }
    }
}
