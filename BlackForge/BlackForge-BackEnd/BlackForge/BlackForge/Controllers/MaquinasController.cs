using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaquinasController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public MaquinasController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/maquinas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Maquina>>> GetMaquinas()
        {
            var maquinas = await _context.Maquinas
                .AsNoTracking()
                .ToListAsync();

            return Ok(maquinas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Maquina>> GetMaquina(int id)
        {
            var maquina = await _context.Maquinas
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maquina == null)
            {
                return NotFound();
            }

            return Ok(maquina);
        }

        [HttpPost]
        public async Task<ActionResult<Maquina>> CriarMaquina(
            Maquina maquina)
        {
            // ================= VALIDAR CAMPOS =================

            if (string.IsNullOrWhiteSpace(maquina.Nome))
            {
                return BadRequest(
                    "O nome da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.Codigo))
            {
                return BadRequest(
                    "O código da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.Fabricante))
            {
                return BadRequest(
                    "O fabricante da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.Modelo))
            {
                return BadRequest(
                    "O modelo da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.NumeroSerie))
            {
                return BadRequest(
                    "O número de série da máquina é obrigatório.");
            }

            // ================= VALIDAR CÓDIGO =================

            var codigoExiste = await _context.Maquinas
                .AnyAsync(m => m.Codigo == maquina.Codigo);

            if (codigoExiste)
            {
                return Conflict(
                    "Já existe uma máquina com esse código.");
            }

            // ================= VALIDAR NÚMERO DE SÉRIE =================

            var numeroSerieExiste = await _context.Maquinas
                .AnyAsync(m => m.NumeroSerie == maquina.NumeroSerie);

            if (numeroSerieExiste)
            {
                return Conflict(
                    "Já existe uma máquina com esse número de série.");
            }

            // ================= CADASTRAR =================

            _context.Maquinas.Add(maquina);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMaquina),
                new { id = maquina.Id },
                maquina
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarMaquina(
            int id,
            Maquina maquina)
        {
            if (id != maquina.Id)
            {
                return BadRequest();
            }

            // ================= VALIDAR CAMPOS =================

            if (string.IsNullOrWhiteSpace(maquina.Nome))
            {
                return BadRequest(
                    "O nome da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.Codigo))
            {
                return BadRequest(
                    "O código da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.Fabricante))
            {
                return BadRequest(
                    "O fabricante da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.Modelo))
            {
                return BadRequest(
                    "O modelo da máquina é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(maquina.NumeroSerie))
            {
                return BadRequest(
                    "O número de série da máquina é obrigatório.");
            }

            // ================= VERIFICAR EXISTÊNCIA =================

            var maquinaExiste = await _context.Maquinas
                .AnyAsync(m => m.Id == id);

            if (!maquinaExiste)
            {
                return NotFound();
            }

            // ================= VALIDAR CÓDIGO =================

            var codigoEmUso = await _context.Maquinas
                .AnyAsync(m =>
                    m.Codigo == maquina.Codigo &&
                    m.Id != id);

            if (codigoEmUso)
            {
                return Conflict(
                    "Já existe outra máquina com esse código.");
            }

            // ================= VALIDAR NÚMERO DE SÉRIE =================

            var numeroSerieEmUso = await _context.Maquinas
                .AnyAsync(m =>
                    m.NumeroSerie == maquina.NumeroSerie &&
                    m.Id != id);

            if (numeroSerieEmUso)
            {
                return Conflict(
                    "Já existe outra máquina com esse número de série.");
            }

            // ================= ATUALIZAR =================

            _context.Entry(maquina).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaquinaExiste(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirMaquina(int id)
        {
            var maquina = await _context.Maquinas
                .FindAsync(id);

            if (maquina == null)
            {
                return NotFound();
            }

            _context.Maquinas.Remove(maquina);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MaquinaExiste(int id)
        {
            return _context.Maquinas
                .Any(m => m.Id == id);
        }
    }
}