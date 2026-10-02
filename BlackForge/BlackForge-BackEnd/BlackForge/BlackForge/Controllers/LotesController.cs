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
        private const string TipoEntrada = "Entrada";

        private readonly BlackForgeDbContext _context;

        public LotesController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/lotes  (opcional: ?materialId=3)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoteDto>>> GetLotes([FromQuery] int? materialId)
        {
            IQueryable<Lote> query = _context.Lotes
                .AsNoTracking()
                .Include(l => l.Material);

            if (materialId.HasValue)
                query = query.Where(l => l.MaterialId == materialId.Value);

            var lotes = await query
                .OrderByDescending(l => l.DataEntrada)
                .ThenByDescending(l => l.Id)
                .ToListAsync();

            return Ok(lotes.Select(ParaDto).ToList());
        }

        // GET: api/lotes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LoteDto>> GetLote(int id)
        {
            var lote = await _context.Lotes
                .AsNoTracking()
                .Include(l => l.Material)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lote == null)
                return NotFound();

            return Ok(ParaDto(lote));
        }

        // POST: api/lotes  (recebimento de um novo lote + movimentação de entrada)
        [HttpPost]
        public async Task<ActionResult<LoteDto>> CriarLote(LoteCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest("O código do lote é obrigatório.");

            if (dto.Codigo.Trim().Length > 50)
                return BadRequest("O código do lote deve ter no máximo 50 caracteres.");

            if (dto.Quantidade <= 0)
                return BadRequest("A quantidade deve ser maior que zero.");

            if (dto.CustoUnitario < 0)
                return BadRequest("O custo unitário não pode ser negativo.");

            var erroDatas = ValidarDatas(dto.DataFabricacao, dto.DataValidade);
            if (erroDatas != null)
                return BadRequest(erroDatas);

            var material = await _context.Materiais
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId);

            if (material == null)
                return BadRequest("O material informado não existe.");

            var codigo = dto.Codigo.Trim();

            var codigoExiste = await _context.Lotes
                .AnyAsync(l => l.MaterialId == dto.MaterialId && l.Codigo == codigo);

            if (codigoExiste)
                return Conflict("Já existe um lote com esse código para este material.");

            var agora = DateTime.Now;

            var observacoes = string.IsNullOrWhiteSpace(dto.Observacoes)
    ? null
    : dto.Observacoes.Trim();

            var lote = new Lote
            {
                Codigo = codigo,
                Quantidade = dto.Quantidade,
                CustoUnitario = dto.CustoUnitario,
                DataEntrada = dto.DataEntrada ?? agora,
                DataFabricacao = dto.DataFabricacao,
                DataValidade = dto.DataValidade,
                Observacoes = observacoes,
                MaterialId = dto.MaterialId,
                Material = material
            };

            var movimentacao = new Movimentacao
            {
                Tipo = TipoEntrada,
                Quantidade = dto.Quantidade,
                DataMovimentacao = agora,
                Observacoes = observacoes,
                MaterialId = dto.MaterialId,
                Lote = lote
            };

            _context.Lotes.Add(lote);
            _context.Movimentacoes.Add(movimentacao);

            // Um único SaveChanges: lote e movimentação são gravados juntos ou nenhum dos dois
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLote), new { id = lote.Id }, ParaDto(lote));
        }

        // PUT: api/lotes/5  (não altera a quantidade)
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarLote(int id, LoteUpdateDto dto)
        {
            var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == id);

            if (lote == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest("O código do lote é obrigatório.");

            if (dto.Codigo.Trim().Length > 50)
                return BadRequest("O código do lote deve ter no máximo 50 caracteres.");

            if (dto.CustoUnitario < 0)
                return BadRequest("O custo unitário não pode ser negativo.");

            var erroDatas = ValidarDatas(dto.DataFabricacao, dto.DataValidade);
            if (erroDatas != null)
                return BadRequest(erroDatas);

            var codigo = dto.Codigo.Trim();

            var codigoExiste = await _context.Lotes
                .AnyAsync(l =>
                    l.MaterialId == lote.MaterialId &&
                    l.Codigo == codigo &&
                    l.Id != id);

            if (codigoExiste)
                return Conflict("Já existe outro lote com esse código para este material.");

            lote.Codigo = codigo;
            lote.CustoUnitario = dto.CustoUnitario;
            lote.DataFabricacao = dto.DataFabricacao;
            lote.DataValidade = dto.DataValidade;
            lote.Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes)
    ? null
    : dto.Observacoes.Trim();

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/lotes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirLote(int id)
        {
            var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == id);

            if (lote == null)
                return NotFound();

            var movimentacoes = await _context.Movimentacoes
                .Where(m => m.LoteId == id)
                .ToListAsync();

            // Só dá para excluir lote que nunca teve saída: senão o histórico ficaria incoerente
            if (movimentacoes.Any(m => m.Tipo != TipoEntrada))
                return Conflict(
                    "Não é possível excluir: o lote já possui saídas ou ajustes registrados.");

            _context.Movimentacoes.RemoveRange(movimentacoes);
            _context.Lotes.Remove(lote);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= AUXILIARES =================

        private static string? ValidarDatas(DateTime? fabricacao, DateTime? validade)
        {
            if (fabricacao.HasValue && validade.HasValue && validade.Value < fabricacao.Value)
                return "A data de validade não pode ser anterior à data de fabricação.";

            return null;
        }

        private static string CalcularStatus(DateTime? validade)
        {
            if (!validade.HasValue)
                return "sem-validade";

            var hoje = DateTime.Today;

            if (validade.Value.Date < hoje)
                return "vencido";

            if (validade.Value.Date <= hoje.AddDays(30))
                return "proximo";

            return "ok";
        }

        private static LoteDto ParaDto(Lote l)
        {
            return new LoteDto
            {
                Id = l.Id,
                Codigo = l.Codigo,
                MaterialId = l.MaterialId,
                MaterialCodigo = l.Material.Codigo,
                MaterialNome = l.Material.Nome,
                Unidade = l.Material.Unidade,
                Quantidade = l.Quantidade,
                CustoUnitario = l.CustoUnitario,
                DataEntrada = l.DataEntrada,
                DataFabricacao = l.DataFabricacao,
                DataValidade = l.DataValidade,
                Observacoes = l.Observacoes,
                Status = CalcularStatus(l.DataValidade)
            };
        }
    }
}