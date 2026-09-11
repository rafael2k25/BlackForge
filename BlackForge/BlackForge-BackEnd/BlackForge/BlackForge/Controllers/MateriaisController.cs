using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MateriaisController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public MateriaisController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/materiais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetMateriais()
        {
            var materiais = await _context.Materiais
                .AsNoTracking()
                .ToListAsync();

            return Ok(materiais);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetMaterial(int id)
        {
            var material = await _context.Materiais
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                return NotFound();
            }

            return Ok(material);
        }

        [HttpPost]
        public async Task<ActionResult<Material>> CriarMaterial(Material material)
        {
            _context.Materiais.Add(material);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMaterial),
                new { id = material.Id },
                material
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarMaterial(
            int id,
            Material material)
        {
            if (id != material.Id)
            {
                return BadRequest();
            }

            _context.Entry(material).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialExiste(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirMaterial(int id)
        {
            var material = await _context.Materiais
                .FindAsync(id);

            if (material == null)
            {
                return NotFound();
            }

            _context.Materiais.Remove(material);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaterialExiste(int id)
        {
            return _context.Materiais
                .Any(m => m.Id == id);
        }
    }
}