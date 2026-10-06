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

            var query = _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .Include(o => o.ProcessosProducao)
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
                    string statusCalculado;

                    if (o.ProcessosProducao == null ||
                        !o.ProcessosProducao.Any())
                    {
                        statusCalculado = "pendente";
                    }
                    else if (o.ProcessosProducao.Any(p =>
                        p.Status == "EM_EXECUCAO"))
                    {
                        statusCalculado = "em_execucao";
                    }
                    else if (o.ProcessosProducao.All(p =>
                        p.Status == "CONCLUIDO"))
                    {
                        statusCalculado = "concluida";
                    }
                    else
                    {
                        statusCalculado = "pendente";
                    }

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
                        Status = statusCalculado,
                        ValorTotal = o.ValorTotal
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim().ToLower();

                itens = itens
                    .Where(i => i.Status == status)
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
                    Quantidade = itens.Count(i =>
                        i.Status == "em_execucao")
                },

                new RelatorioStatusDTO
                {
                    Status = "Concluída",
                    Quantidade = itens.Count(i =>
                        i.Status == "concluida")
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
    }
}