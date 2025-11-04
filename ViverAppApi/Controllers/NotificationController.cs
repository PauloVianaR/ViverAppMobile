using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly ViverappmobileContext _context;

        public NotificationController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await _context.Notifications.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        [HttpPost]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Idnotification }, notification);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotification(int id, Notification notification)
        {
            if (id != notification.Idnotification)
            {
                return BadRequest();
            }

            var existingnotification = await _context.Notifications.FirstOrDefaultAsync(n => n.Idnotification == id);
            if (existingnotification is null)
                return NotFound();

            existingnotification.Read = notification.Read;
            existingnotification.Sent = notification.Sent;
            existingnotification.Severity = notification.Severity;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
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
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Idnotification == id);
        }
    }
}
