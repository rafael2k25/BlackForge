using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    // Registra consumo de material como se uma máquina tivesse usado.
    [ApiController]
    [Route("api/[controller]")]
    public class SimulacaoController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public SimulacaoController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // POST /api/simulacao/consumo
        [HttpPost("consumo")]
        public async Task<ActionResult<SimulacaoConsumoResultadoDTO>> SimularConsumo(
            [FromBody] SimulacaoConsumoDTO dto)
        {
            if (dto.Quantidade <= 0)
                return BadRequest("A quantidade deve ser maior que zero.");

            var material = await _context.Materiais
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == dto.MaterialId);

            if (material == null)
                return NotFound("Material não encontrado.");

            // Vence primeiro, sai primeiro. Sem validade vai por último.
            var lotes = await _context.Lotes
                .Where(l => l.MaterialId == dto.MaterialId && l.Quantidade > 0)
                .OrderBy(l => l.DataValidade == null)
                .ThenBy(l => l.DataValidade)
                .ThenBy(l => l.DataEntrada)
                .ToListAsync();

            var disponivel = lotes.Sum(l => l.Quantidade);

            if (disponivel < dto.Quantidade)
                return BadRequest(
                    $"Estoque insuficiente. Disponível: {disponivel} {material.Unidade}."
                );

            var data = dto.DataMovimentacao ?? DateTime.Now;

            var observacao = string.IsNullOrWhiteSpace(dto.Descricao)
                ? null
                : dto.Descricao.Trim();

            var restante = dto.Quantidade;
            var retiradas = new List<SimulacaoRetiradaDTO>();

            using var transacao = await _context.Database.BeginTransactionAsync();

            foreach (var lote in lotes)
            {
                if (restante <= 0)
                    break;

                var retirar = Math.Min(lote.Quantidade, restante);

                lote.Quantidade -= retirar;
                restante -= retirar;

                _context.Movimentacoes.Add(new Movimentacao
                {                 
                    Tipo = "saida",
                    Quantidade = retirar,
                    DataMovimentacao = data,
                    Observacoes = observacao,
                    MaterialId = material.Id,
                    LoteId = lote.Id
                });

                retiradas.Add(new SimulacaoRetiradaDTO
                {
                    Lote = lote.Codigo,
                    Quantidade = retirar
                });
            }

            await _context.SaveChangesAsync();
            await transacao.CommitAsync();

            return Ok(new SimulacaoConsumoResultadoDTO
            {
                Material = material.Nome,
                Unidade = material.Unidade,
                QuantidadeConsumida = dto.Quantidade,
                EstoqueRestante = disponivel - dto.Quantidade,
                Lotes = retiradas
            });
        }
    }

    public class SimulacaoConsumoDTO
    {
        public int MaterialId { get; set; }
        public decimal Quantidade { get; set; }
        public DateTime? DataMovimentacao { get; set; }
        public string? Descricao { get; set; }
    }

    public class SimulacaoConsumoResultadoDTO
    {
        public string Material { get; set; } = "";
        public string Unidade { get; set; } = "";
        public decimal QuantidadeConsumida { get; set; }
        public decimal EstoqueRestante { get; set; }
        public List<SimulacaoRetiradaDTO> Lotes { get; set; } = new();
    }

    public class SimulacaoRetiradaDTO
    {
        public string Lote { get; set; } = "";
        public decimal Quantidade { get; set; }
    }
}