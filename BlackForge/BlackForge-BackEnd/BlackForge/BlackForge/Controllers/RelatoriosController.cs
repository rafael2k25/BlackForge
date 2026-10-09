using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private static readonly string[] StatusValidos =
            { "pendente", "em_execucao", "concluida" };

        private readonly BlackForgeDbContext _context;

        public RelatoriosController(BlackForgeDbContext context)
        {
            _context = context;
        }

        [HttpGet("servicos")]
        public async Task<ActionResult<RelatorioServicosDTO>> GetRelatorioServicos(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] string? status)
        {
            if (dataInicio.HasValue &&
                dataFim.HasValue &&
                dataInicio.Value.Date > dataFim.Value.Date)
            {
                return BadRequest(
                    "A data inicial não pode ser maior que a data final."
                );
            }

            // O status vem da própria Ordem de Serviço.
            // ProcessosProducao não é mais consultado aqui.
            var query = _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .AsQueryable();

            if (dataInicio.HasValue)
            {
                var inicio = dataInicio.Value.Date;

                query = query.Where(o =>
                    o.DataAbertura >= inicio);
            }

            if (dataFim.HasValue)
            {
                var fim = dataFim.Value.Date.AddDays(1);

                query = query.Where(o =>
                    o.DataAbertura < fim);
            }

            var ordens = await query
                .OrderByDescending(o => o.DataAbertura)
                .ToListAsync();

            var itens = ordens
                .Select(o =>
                {
                    var statusOS = (o.Status ?? string.Empty)
                        .Trim()
                        .ToLowerInvariant();

                    if (!StatusValidos.Contains(statusOS))
                        statusOS = "pendente";

                    return new RelatorioServicoItemDTO
                    {
                        Id = o.Id,
                        NumeroOS = o.NumeroOS,
                        Cliente = o.Cliente,
                        TipoServico = o.TipoServico,
                        Responsavel = o.Funcionario != null
                            ? o.Funcionario.Nome
                            : "Não informado",
                        DataAbertura = o.DataAbertura,
                        DataEntrega = o.DataEntrega,
                        Status = statusOS,
                        ValorTotal = o.ValorTotal
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var filtro = status.Trim().ToLowerInvariant();

                itens = itens
                    .Where(i => i.Status == filtro)
                    .ToList();
            }

            var totalOS = itens.Count;

            var emExecucao = itens.Count(i =>
                i.Status == "em_execucao");

            var concluidas = itens.Count(i =>
                i.Status == "concluida");

            var valorTotal = itens.Sum(i =>
                i.ValorTotal);

            var porStatus = new List<RelatorioStatusDTO>
            {
                new RelatorioStatusDTO
                {
                    Status = "Pendente",
                    Quantidade = itens.Count(i =>
                        i.Status == "pendente")
                },

                new RelatorioStatusDTO
                {
                    Status = "Em execução",
                    Quantidade = emExecucao
                },

                new RelatorioStatusDTO
                {
                    Status = "Concluída",
                    Quantidade = concluidas
                }
            };

            var porTipo = itens
                .GroupBy(i => i.TipoServico)
                .Select(g => new RelatorioTipoServicoDTO
                {
                    TipoServico = string.IsNullOrWhiteSpace(g.Key)
                        ? "Não informado"
                        : g.Key,

                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ToList();

            var resultado = new RelatorioServicosDTO
            {
                TotalOS = totalOS,
                EmExecucao = emExecucao,
                Concluidas = concluidas,
                ValorTotal = valorTotal,
                PorStatus = porStatus,
                PorTipo = porTipo,
                Registros = itens
            };

            return Ok(resultado);
        }

        private static readonly string[] TiposValidos = { "entrada", "saida" };

        private static string NormalizarTipo(string? tipo)
        {
            var texto = (tipo ?? string.Empty).Trim().ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD);

            var semAcento = new string(texto
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                            != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());

            return semAcento;
        }

        [HttpGet("estoque")]
        public async Task<ActionResult<RelatorioEstoqueDTO>> GetRelatorioEstoque(
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim,
            [FromQuery] string? tipo,
            [FromQuery] int? materialId)
        {
            if (dataInicio.HasValue && dataFim.HasValue &&
                dataInicio.Value.Date > dataFim.Value.Date)
                return BadRequest("A data inicial não pode ser maior que a data final.");

            var filtroTipo = NormalizarTipo(tipo);

            if (filtroTipo != "" && !TiposValidos.Contains(filtroTipo))
                return BadRequest("Tipo de movimentação inválido.");

            var query = _context.Movimentacoes
                .AsNoTracking()
                .Include(m => m.Material)
                .Include(m => m.Lote)
                .AsQueryable();

            if (dataInicio.HasValue)
            {
                var inicio = dataInicio.Value.Date;
                query = query.Where(m => m.DataMovimentacao >= inicio);
            }

            if (dataFim.HasValue)
            {
                var fim = dataFim.Value.Date.AddDays(1);
                query = query.Where(m => m.DataMovimentacao < fim);
            }

            if (materialId.HasValue)
                query = query.Where(m => m.MaterialId == materialId.Value);

            var movimentacoes = await query
                .OrderByDescending(m => m.DataMovimentacao)
                .ToListAsync();

            var itens = movimentacoes
                .Select(m =>
                {
                    var custo = m.Lote?.CustoUnitario ?? 0m;

                    return new RelatorioMovimentacaoItemDTO
                    {
                        Id = m.Id,
                        Data = m.DataMovimentacao,
                        Material = m.Material?.Nome ?? "Não informado",
                        Lote = m.Lote?.Codigo ?? "-",
                        Tipo = NormalizarTipo(m.Tipo),
                        Quantidade = m.Quantidade,
                        Unidade = m.Material?.Unidade ?? "",
                        CustoUnitario = custo,
                        Total = m.Quantidade * custo
                    };
                })
                .ToList();

            if (filtroTipo != "")
                itens = itens.Where(i => i.Tipo == filtroTipo).ToList();

            var saldos = await _context.Lotes
                .AsNoTracking()
                .GroupBy(l => l.MaterialId)
                .Select(g => new { MaterialId = g.Key, Saldo = g.Sum(l => l.Quantidade) })
                .ToDictionaryAsync(x => x.MaterialId, x => x.Saldo);

            var materiaisQuery = _context.Materiais.AsNoTracking().AsQueryable();

            if (materialId.HasValue)
                materiaisQuery = materiaisQuery.Where(m => m.Id == materialId.Value);

            var materiais = await materiaisQuery
                .Select(m => new { m.Id, m.EstoqueMinimo })
                .ToListAsync();

            var abaixoDoMinimo = materiais.Count(m =>
                saldos.GetValueOrDefault(m.Id) < m.EstoqueMinimo);

            var porMes = itens.Count > 0 &&
                (itens.Max(i => i.Data) - itens.Min(i => i.Data)).TotalDays > 60;

            var porPeriodo = itens
                .GroupBy(i => porMes
                    ? new DateTime(i.Data.Year, i.Data.Month, 1)
                    : i.Data.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RelatorioMovimentacaoPeriodoDTO
                {
                    Periodo = g.Key.ToString(porMes ? "MM/yyyy" : "dd/MM"),
                    Entradas = g.Where(i => i.Tipo == "entrada").Sum(i => i.Quantidade),
                    Saidas = g.Where(i => i.Tipo == "saida").Sum(i => i.Quantidade)
                })
                .ToList();

            var consumo = itens
                .Where(i => i.Tipo == "saida")
                .GroupBy(i => new { i.Material, i.Unidade })
                .Select(g => new RelatorioConsumoMaterialDTO
                {
                    Material = g.Key.Material,
                    Unidade = g.Key.Unidade,
                    Quantidade = g.Sum(i => i.Quantidade)
                })
                .OrderByDescending(x => x.Quantidade)
                .Take(10)
                .ToList();

            return Ok(new RelatorioEstoqueDTO
            {
                TotalMateriais = materiais.Count,
                TotalEntradas = itens.Where(i => i.Tipo == "entrada").Sum(i => i.Quantidade),
                TotalSaidas = itens.Where(i => i.Tipo == "saida").Sum(i => i.Quantidade),
                AbaixoDoMinimo = abaixoDoMinimo,
                PorPeriodo = porPeriodo,
                ConsumoPorMaterial = consumo,
                Registros = itens
            });
        }
    }
}