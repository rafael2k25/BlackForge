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
                .Include(o => o.Materiais)
                    .ThenInclude(om => om.Material)
                .OrderByDescending(o => o.DataAbertura)
                .ToListAsync();

            return Ok(ordens);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrdemServico>> GetOrdemServico(int id)
        {
            var ordem = await _context.OrdensServico
                .AsNoTracking()
                .Include(o => o.Funcionario)
                .Include(o => o.Materiais)
                    .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordem == null)
                return NotFound();

            return Ok(ordem);
        }

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
                return Conflict("Já existe uma ordem de serviço com esse número.");

            if (ordem.FuncionarioId.HasValue)
            {
                var funcionarioExiste = await _context.Funcionarios
                    .AnyAsync(f => f.Id == ordem.FuncionarioId.Value);

                if (!funcionarioExiste)
                    return BadRequest("O funcionário responsável informado não existe.");
            }

            if (ordem.DataAbertura == default)
                ordem.DataAbertura = DateTime.Now;

            if (ordem.Materiais != null && ordem.Materiais.Any())
            {
                foreach (var item in ordem.Materiais)
                {
                    if (item.Quantidade <= 0)
                        return BadRequest("A quantidade do material deve ser maior que zero.");

                    if (item.ValorUnitario < 0)
                        return BadRequest("O valor unitário do material não pode ser negativo.");

                    var materialExiste = await _context.Materiais
                        .AnyAsync(m => m.Id == item.MaterialId);

                    if (!materialExiste)
                        return BadRequest(
                            $"O material com ID {item.MaterialId} não existe."
                        );

                    // Calcula o subtotal automaticamente
                    item.Subtotal = item.Quantidade * item.ValorUnitario;
                }
            }

            ordem.ValorMateriais = ordem.Materiais?
                .Sum(m => m.Subtotal) ?? 0;

            if (ordem.ValorMaoObra < 0)
                return BadRequest("O valor da mão de obra não pode ser negativo.");

            if (ordem.Desconto < 0)
                return BadRequest("O desconto não pode ser negativo.");

            ordem.ValorTotal =
                ordem.ValorMateriais +
                ordem.ValorMaoObra -
                ordem.Desconto;

            if (ordem.ValorTotal < 0)
                ordem.ValorTotal = 0;

            _context.OrdensServico.Add(ordem);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrdemServico),
                new { id = ordem.Id },
                ordem
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarOrdemServico(
            int id,
            OrdemServico ordem)
        {
            if (id != ordem.Id)
                return BadRequest();

            var ordemExistente = await _context.OrdensServico
                .Include(o => o.Materiais)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (ordemExistente == null)
                return NotFound();

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

            if (ordem.Materiais != null && ordem.Materiais.Any())
            {
                foreach (var item in ordem.Materiais)
                {
                    if (item.Quantidade <= 0)
                        return BadRequest(
                            "A quantidade do material deve ser maior que zero."
                        );

                    if (item.ValorUnitario < 0)
                        return BadRequest(
                            "O valor unitário do material não pode ser negativo."
                        );

                    var materialExiste = await _context.Materiais
                        .AnyAsync(m => m.Id == item.MaterialId);

                    if (!materialExiste)
                        return BadRequest(
                            $"O material com ID {item.MaterialId} não existe."
                        );

                    item.Subtotal =
                        item.Quantidade *
                        item.ValorUnitario;
                }
            }

            ordem.ValorMateriais = ordem.Materiais?
                .Sum(m => m.Subtotal) ?? 0;

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
           
            ordemExistente.NumeroOS = ordem.NumeroOS;
            ordemExistente.Cliente = ordem.Cliente;
            ordemExistente.Contato = ordem.Contato;
            ordemExistente.Endereco = ordem.Endereco;
            ordemExistente.DataAbertura = ordem.DataAbertura;
            ordemExistente.DescricaoServico = ordem.DescricaoServico;
            ordemExistente.TipoServico = ordem.TipoServico;
            ordemExistente.DataInicio = ordem.DataInicio;
            ordemExistente.DataEntrega = ordem.DataEntrega;
            ordemExistente.FuncionarioId = ordem.FuncionarioId;
            ordemExistente.ValorMateriais = ordem.ValorMateriais;
            ordemExistente.ValorMaoObra = ordem.ValorMaoObra;
            ordemExistente.Desconto = ordem.Desconto;
            ordemExistente.ValorTotal = ordem.ValorTotal;
            ordemExistente.CondicaoPagamento = ordem.CondicaoPagamento;
            ordemExistente.Observacoes = ordem.Observacoes;

            _context.OrdensServicoMateriais.RemoveRange(
                ordemExistente.Materiais
            );

            if (ordem.Materiais != null && ordem.Materiais.Any())
            {
                foreach (var item in ordem.Materiais)
                {
                    item.Id = 0;
                    item.OrdemServicoId = id;

                    _context.OrdensServicoMateriais.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

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