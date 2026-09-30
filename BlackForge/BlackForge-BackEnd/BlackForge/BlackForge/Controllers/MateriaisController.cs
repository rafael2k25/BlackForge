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
        public async Task<ActionResult<IEnumerable<MaterialDto>>> GetMateriais()
        {
            var materiais = await _context.Materiais
                .AsNoTracking()
                .OrderBy(m => m.Nome)
                .ToListAsync();

            var saldos = await ObterSaldosAsync();

            return Ok(materiais.Select(m => ParaDto(m, saldos)).ToList());
        }

        // GET: api/materiais/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialDto>> GetMaterial(int id)
        {
            var material = await _context.Materiais
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
                return NotFound();

            var saldos = await ObterSaldosAsync(id);

            return Ok(ParaDto(material, saldos));
        }

        // POST: api/materiais
        [HttpPost]
        public async Task<ActionResult<MaterialDto>> CriarMaterial(MaterialCreateDto dto)
        {
            var erro = Validar(dto);
            if (erro != null)
                return BadRequest(erro);

            var codigo = dto.Codigo.Trim();

            var codigoExiste = await _context.Materiais
                .AnyAsync(m => m.Codigo == codigo);

            if (codigoExiste)
                return Conflict("Já existe um material com esse código.");

            var material = new Material
            {
                Codigo = codigo,
                Nome = dto.Nome.Trim(),
                Categoria = dto.Categoria.Trim(),
                Unidade = dto.Unidade.Trim(),
                Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim(),
                EstoqueMinimo = dto.EstoqueMinimo
            };

            _context.Materiais.Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMaterial),
                new { id = material.Id },
                ParaDto(material, new()));
        }

        // PUT: api/materiais/5
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarMaterial(int id, MaterialCreateDto dto)
        {
            var material = await _context.Materiais.FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
                return NotFound();

            var erro = Validar(dto);
            if (erro != null)
                return BadRequest(erro);

            var codigo = dto.Codigo.Trim();

            var codigoExiste = await _context.Materiais
                .AnyAsync(m => m.Codigo == codigo && m.Id != id);

            if (codigoExiste)
                return Conflict("Já existe outro material com esse código.");

            var unidade = dto.Unidade.Trim();

            // Trocar a unidade com estoque lançado deixaria as quantidades sem sentido
            if (unidade != material.Unidade &&
                await _context.Lotes.AnyAsync(l => l.MaterialId == id))
            {
                return BadRequest(
                    "Não é possível alterar a unidade de um material que já possui lotes.");
            }

            material.Codigo = codigo;
            material.Nome = dto.Nome.Trim();
            material.Categoria = dto.Categoria.Trim();
            material.Unidade = unidade;
            material.Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim();
            material.EstoqueMinimo = dto.EstoqueMinimo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/materiais/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirMaterial(int id)
        {
            var material = await _context.Materiais.FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
                return NotFound();

            var emUso =
                await _context.Lotes.AnyAsync(l => l.MaterialId == id) ||
                await _context.Movimentacoes.AnyAsync(m => m.MaterialId == id) ||
                await _context.OrdensServicoMateriais.AnyAsync(om => om.MaterialId == id);

            if (emUso)
                return Conflict(
                    "Não é possível excluir: o material possui lotes, movimentações ou está em uma ordem de serviço.");

            _context.Materiais.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ================= AUXILIARES =================

        private static string? Validar(MaterialCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Codigo))
                return "O código do material é obrigatório.";

            if (string.IsNullOrWhiteSpace(dto.Nome))
                return "O nome do material é obrigatório.";

            if (string.IsNullOrWhiteSpace(dto.Categoria))
                return "A categoria do material é obrigatória.";

            if (string.IsNullOrWhiteSpace(dto.Unidade))
                return "A unidade do material é obrigatória.";

            if (dto.Codigo.Trim().Length > 50)
                return "O código deve ter no máximo 50 caracteres.";

            if (dto.Nome.Trim().Length > 150)
                return "O nome deve ter no máximo 150 caracteres.";

            if (dto.Categoria.Trim().Length > 50)
                return "A categoria deve ter no máximo 50 caracteres.";

            if (dto.Unidade.Trim().Length > 20)
                return "A unidade deve ter no máximo 20 caracteres.";

            if (dto.EstoqueMinimo < 0)
                return "O estoque mínimo não pode ser negativo.";

            return null;
        }

        // Saldo e valor em estoque por material, somando os lotes
        private async Task<Dictionary<int, (decimal Quantidade, decimal Valor)>> ObterSaldosAsync(
            int? materialId = null)
        {
            IQueryable<Lote> query = _context.Lotes.AsNoTracking();

            if (materialId.HasValue)
                query = query.Where(l => l.MaterialId == materialId.Value);

            var grupos = await query
                .GroupBy(l => l.MaterialId)
                .Select(g => new
                {
                    MaterialId = g.Key,
                    Quantidade = g.Sum(l => l.Quantidade),
                    Valor = g.Sum(l => l.Quantidade * l.CustoUnitario)
                })
                .ToListAsync();

            return grupos.ToDictionary(g => g.MaterialId, g => (g.Quantidade, g.Valor));
        }

        private static MaterialDto ParaDto(
            Material m,
            Dictionary<int, (decimal Quantidade, decimal Valor)> saldos)
        {
            saldos.TryGetValue(m.Id, out var saldo);

            // Custo médio ponderado pelo saldo atual dos lotes
            var custoMedio = saldo.Quantidade > 0
                ? Math.Round(saldo.Valor / saldo.Quantidade, 2)
                : 0m;

            return new MaterialDto
            {
                Id = m.Id,
                Codigo = m.Codigo,
                Nome = m.Nome,
                Categoria = m.Categoria,
                Unidade = m.Unidade,
                Descricao = m.Descricao,
                EstoqueMinimo = m.EstoqueMinimo,
                QuantidadeTotal = saldo.Quantidade,
                CustoMedio = custoMedio,
                AbaixoDoMinimo = m.EstoqueMinimo > 0 && saldo.Quantidade < m.EstoqueMinimo
            };
        }
    }
}