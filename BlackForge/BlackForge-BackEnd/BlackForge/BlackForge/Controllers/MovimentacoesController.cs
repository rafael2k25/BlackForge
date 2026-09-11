using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public MovimentacoesController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/movimentacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movimentacao>>> GetMovimentacoes()
        {
            var movimentacoes = await _context.Movimentacoes
                .Include(m => m.Material)
                .Include(m => m.Lote)
                .AsNoTracking()
                .OrderByDescending(m => m.DataMovimentacao)
                .ToListAsync();

            return Ok(movimentacoes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Movimentacao>> GetMovimentacao(int id)
        {
            var movimentacao = await _context.Movimentacoes
                .Include(m => m.Material)
                .Include(m => m.Lote)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimentacao == null)
            {
                return NotFound();
            }

            return Ok(movimentacao);
        }

        [HttpPost]
        public async Task<ActionResult<Movimentacao>> CriarMovimentacao(
            Movimentacao movimentacao)
        {
            if (movimentacao.Tipo != "entrada" &&
                movimentacao.Tipo != "saida" &&
                movimentacao.Tipo != "ajuste")
            {
                return BadRequest(
                    "O tipo deve ser 'entrada', 'saida' ou 'ajuste'.");
            }

            if (movimentacao.Quantidade <= 0)
            {
                return BadRequest(
                    "A quantidade deve ser maior que zero.");
            }

            var materialExiste = await _context.Materiais
                .AnyAsync(m => m.Id == movimentacao.MaterialId);

            if (!materialExiste)
            {
                return BadRequest(
                    "O material informado não existe.");
            }

            if (movimentacao.LoteId.HasValue)
            {
                var lote = await _context.Lotes
                    .FirstOrDefaultAsync(l =>
                        l.Id == movimentacao.LoteId.Value);

                if (lote == null)
                {
                    return BadRequest(
                        "O lote informado não existe.");
                }

                if (lote.MaterialId != movimentacao.MaterialId)
                {
                    return BadRequest(
                        "O lote informado não pertence ao material selecionado.");
                }

                if (movimentacao.Tipo == "entrada")
                {
                    lote.Quantidade += movimentacao.Quantidade;
                }
                else if (movimentacao.Tipo == "saida")
                {
                    if (lote.Quantidade < movimentacao.Quantidade)
                    {
                        return BadRequest(
                            "Quantidade insuficiente no lote.");
                    }

                    lote.Quantidade -= movimentacao.Quantidade;
                }
                else if (movimentacao.Tipo == "ajuste")
                {
                    lote.Quantidade = movimentacao.Quantidade;
                }
            }
      
            if (movimentacao.DataMovimentacao == default)
            {
                movimentacao.DataMovimentacao = DateTime.Now;
            }

            _context.Movimentacoes.Add(movimentacao);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMovimentacao),
                new { id = movimentacao.Id },
                movimentacao
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarMovimentacao(
            int id,
            Movimentacao movimentacao)
        {
            if (id != movimentacao.Id)
            {
                return BadRequest();
            }

            var movimentacaoExistente = await _context.Movimentacoes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimentacaoExistente == null)
            {
                return NotFound();
            }

            if (movimentacao.Quantidade <= 0)
            {
                return BadRequest(
                    "A quantidade deve ser maior que zero.");
            }

            var materialExiste = await _context.Materiais
                .AnyAsync(m => m.Id == movimentacao.MaterialId);

            if (!materialExiste)
            {
                return BadRequest(
                    "O material informado não existe.");
            }

            if (movimentacao.LoteId.HasValue)
            {
                var loteExiste = await _context.Lotes
                    .AnyAsync(l =>
                        l.Id == movimentacao.LoteId.Value &&
                        l.MaterialId == movimentacao.MaterialId);

                if (!loteExiste)
                {
                    return BadRequest(
                        "O lote informado não existe ou não pertence ao material.");
                }
            }

            _context.Entry(movimentacao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovimentacaoExiste(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }
   
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirMovimentacao(int id)
        {
            var movimentacao = await _context.Movimentacoes
                .FindAsync(id);

            if (movimentacao == null)
            {
                return NotFound();
            }

            _context.Movimentacoes.Remove(movimentacao);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovimentacaoExiste(int id)
        {
            return _context.Movimentacoes
                .Any(m => m.Id == id);
        }
    }
}