using BlackForge.Data;
using BlackForge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackForge.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdensServicoController : ControllerBase
    {
        private readonly BlackForgeDbContext _context;

        public OrdensServicoController(BlackForgeDbContext context)
        {
            _context = context;
        }

        // GET: api/ordensservico
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdemServico>>> GetOrdensServico()
        {
            var ordens = await _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .OrderByDescending(o => o.DataAbertura)
                .ToListAsync();

            return Ok(ordens);
        }

        // GET: api/ordensservico/1
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdemServico>> GetOrdemServico(int id)
        {
            var ordem = await _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound();

            return Ok(ordem);
        }

        // POST: api/ordensservico
        [HttpPost]
        public async Task<ActionResult<OrdemServico>> CriarOrdemServico(
            OrdemServico ordem)

        {
            if (string.IsNullOrWhiteSpace(ordem.NumeroOS))
                return BadRequest("O número da OS é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.Cliente))
                return BadRequest("O cliente é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.DescricaoServico))
                return BadRequest("A descrição do serviço é obrigatória.");

            if (string.IsNullOrWhiteSpace(ordem.TipoServico))
                return BadRequest("O tipo de serviço é obrigatório.");

            var numeroExiste = await _context.OrdensServico
                .AnyAsync(o => o.NumeroOS == ordem.NumeroOS);

            if (numeroExiste)
                return Conflict(
                    "Já existe uma ordem de serviço com esse número."
                );

            if (ordem.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == ordem.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest(
                        "O funcionário responsável informado não existe."
                    );
            }

            if (ordem.DataAbertura == default)
                ordem.DataAbertura = DateTime.Now;

            if (ordem.ValorMateriais < 0)
                return BadRequest(
                    "O valor dos materiais não pode ser negativo."
                );

            if (ordem.ValorMaoObra < 0)
                return BadRequest(
                    "O valor da mão de obra não pode ser negativo."
                );

            if (ordem.Desconto < 0)
                return BadRequest(
                    "O desconto não pode ser negativo."
                );

            ordem.ValorTotal =
                ordem.ValorMateriais +
                ordem.ValorMaoObra -
                ordem.Desconto;

            if (ordem.ValorTotal < 0)
                ordem.ValorTotal = 0;

            ordem.Status = "pendente";
            ordem.DataConclusao = null;

            _context.OrdensServico.Add(ordem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrdemServico),
                new { id = ordem.Id },
                ordem
            );
        }

        // PUT: api/ordensservico/1
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarOrdemServico(
            int id,
            OrdemServico ordem)
        {
            if (id != ordem.Id)
                return BadRequest(
                    "O ID da URL não corresponde ao ID da ordem."
                );

            var ordemExistente = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordemExistente == null)
                return NotFound();

            if (ordemExistente.Status == "concluida")
                return Conflict(
                    "Uma OS concluída não pode ser editada. Reabra a OS antes de alterar."
                );

            if (string.IsNullOrWhiteSpace(ordem.NumeroOS))
                return BadRequest("O número da OS é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.Cliente))
                return BadRequest("O cliente é obrigatório.");

            if (string.IsNullOrWhiteSpace(ordem.DescricaoServico))
                return BadRequest("A descrição do serviço é obrigatória.");

            if (string.IsNullOrWhiteSpace(ordem.TipoServico))
                return BadRequest("O tipo de serviço é obrigatório.");

            var numeroExiste = await _context.OrdensServico
                .AnyAsync(o =>
                    o.NumeroOS == ordem.NumeroOS &&
                    o.Id != id);

            if (numeroExiste)
                return Conflict(
                    "Já existe outra ordem de serviço com esse número."
                );

            if (ordem.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == ordem.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest(
                        "O funcionário responsável informado não existe."
                    );
            }

            if (ordem.ValorMateriais < 0)
                return BadRequest(
                    "O valor dos materiais não pode ser negativo."
                );

            if (ordem.ValorMaoObra < 0)
                return BadRequest(
                    "O valor da mão de obra não pode ser negativo."
                );

            if (ordem.Desconto < 0)
                return BadRequest(
                    "O desconto não pode ser negativo."
                );

            ordemExistente.NumeroOS = ordem.NumeroOS;
            ordemExistente.Cliente = ordem.Cliente;
            ordemExistente.Contato = ordem.Contato;
            ordemExistente.Endereco = ordem.Endereco;
            ordemExistente.DataAbertura = ordem.DataAbertura;
            ordemExistente.DescricaoServico = ordem.DescricaoServico;
            ordemExistente.TipoServico = ordem.TipoServico;
            ordemExistente.DataEntrega = ordem.DataEntrega;
            ordemExistente.FuncionarioId = ordem.FuncionarioId;
            ordemExistente.ValorMateriais = ordem.ValorMateriais;
            ordemExistente.ValorMaoObra = ordem.ValorMaoObra;
            ordemExistente.Desconto = ordem.Desconto;
            ordemExistente.CondicaoPagamento = ordem.CondicaoPagamento;
            ordemExistente.Observacoes = ordem.Observacoes;

            ordemExistente.ValorTotal =
                ordemExistente.ValorMateriais +
                ordemExistente.ValorMaoObra -
                ordemExistente.Desconto;

            if (ordemExistente.ValorTotal < 0)
                ordemExistente.ValorTotal = 0;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static readonly string[] StatusValidos =
        { "pendente", "em_execucao", "concluida" };

        // PATCH: api/ordensservico/1/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> AlterarStatus(
            int id,
            [FromBody] AlterarStatusOSDto dto)
        {
            var status = dto.Status?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(status) || !StatusValidos.Contains(status))
                return BadRequest("Status inválido.");

            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound("Ordem de serviço não encontrada.");

            ordem.Status = status;
            ordem.DataConclusao = status == "concluida" ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            // Retorna só o necessário, evitando o ciclo de serialização
            return Ok(new { ordem.Id, ordem.Status, ordem.DataConclusao });
        }

        // DELETE: api/ordensservico/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirOrdemServico(int id)
        {
            var ordem = await _context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound();

            _context.OrdensServico.Remove(ordem);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}