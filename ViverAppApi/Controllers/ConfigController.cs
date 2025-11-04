using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;

namespace ViverAppApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly ViverappmobileContext _context;

        public ConfigController(ViverappmobileContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Config>>> GetConfigs()
        {
            return await _context.Configs
                .Where(c => c.Canshow == (sbyte)1)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Config>> GetConfig(int id)
        {
            var config = await _context.Configs.FindAsync(id);

            if (config == null)
            {
                return NotFound();
            }

            return config;
        }

        [HttpPost]
        public async Task<ActionResult<Config>> PostConfig(Config config)
        {
            _context.Configs.Add(config);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConfig), new { id = config.Idconfig }, config);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConfig(int id, Config config)
        {
            if (id != config.Idconfig)
            {
                return BadRequest();
            }

            _context.Entry(config).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConfigExists(id))
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
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var config = await _context.Configs.FindAsync(id);
            if (config == null)
            {
                return NotFound();
            }

            _context.Configs.Remove(config);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConfigExists(int id)
        {
            return _context.Configs.Any(e => e.Idconfig == id);
        }
    }
}