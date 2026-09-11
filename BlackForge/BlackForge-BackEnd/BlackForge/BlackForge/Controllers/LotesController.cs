using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotesController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public LotesController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/lotes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lote>>> GetLotes()
        {
            var lotes = await _context.Lotes
                .Include(l => l.Material)
                .AsNoTracking()
                .ToListAsync();

            return Ok(lotes);
        }

        // GET: api/lotes/
        [HttpGet("{id}")]
        public async Task<ActionResult<Lote>> GetLote(int id)
        {
            var lote = await _context.Lotes
                .Include(l => l.Material)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lote == null)
            {
                return NotFound();
            }

            return Ok(lote);
        }

        // POST: api/lotes
        [HttpPost]
        public async Task<ActionResult<Lote>> CriarLote(Lote lote)
        {
            var materialExiste = await _context.Materiais
                .AnyAsync(m => m.Id == lote.MaterialId);

            if (!materialExiste)
            {
                return BadRequest("O material informado não existe.");
            }

            _context.Lotes.Add(lote);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetLote),
                new { id = lote.Id },
                lote
            );
        }

        // PUT: api/lotes/
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarLote(
            int id,
            Lote lote)
        {
            if (id != lote.Id)
            {
                return BadRequest();
            }

            var materialExiste = await _context.Materiais
                .AnyAsync(m => m.Id == lote.MaterialId);

            if (!materialExiste)
            {
                return BadRequest("O material informado não existe.");
            }

            _context.Entry(lote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoteExiste(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/lotes/
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirLote(int id)
        {
            var lote = await _context.Lotes
                .FindAsync(id);

            if (lote == null)
            {
                return NotFound();
            }

            _context.Lotes.Remove(lote);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoteExiste(int id)
        {
            return _context.Lotes
                .Any(l => l.Id == id);
        }
    }
}