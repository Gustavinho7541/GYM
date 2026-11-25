using AcademiaApi.Data;
using AcademiaApi.Dtos;
using AcademiaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Controllers;

/// <summary>
/// Planos de assinatura oferecidos pela academia (CRUD). O plano define o valor mensal e a duração usados no cálculo das matrículas.
/// </summary>
[Route("api/planos")]
public class PlanosController(AcademiaContext db) : ApiControllerBase
{
    /// <summary>Lista os planos.</summary>
    /// <param name="somenteAtivos">Se true, retorna apenas planos que aceitam novas matrículas.</param>
    /// <response code="200">Lista de planos ordenada por duração.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlanoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlanoResponse>>> Listar([FromQuery] bool somenteAtivos = false)
    {
        var query = db.Planos.AsNoTracking();
        if (somenteAtivos) query = query.Where(p => p.Ativo);
        var lista = await query.OrderBy(p => p.DuracaoMeses).ThenBy(p => p.Id).ToListAsync();
        return Ok(lista.Select(p => p.ToResponse()));
    }

    /// <summary>Consulta um plano pelo id.</summary>
    /// <param name="id">Identificador do plano. Exemplo: 4</param>
    /// <response code="200">Plano encontrado.</response>
    /// <response code="404">Não existe plano com o id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlanoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanoResponse>> Obter(int id)
    {
        var plano = await db.Planos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return plano is null ? NaoEncontrado($"Plano {id} não encontrado.") : Ok(plano.ToResponse());
    }

    /// <summary>Cadastra um novo plano.</summary>
    /// <remarks>
    /// Exemplo de corpo:
    ///
    ///     {
    ///       "nome": "Bimestral",
    ///       "descricao": "2 meses de acesso livre à musculação.",
    ///       "valorMensal": 119.90,
    ///       "duracaoMeses": 2,
    ///       "ativo": true
    ///     }
    /// </remarks>
    /// <response code="201">Plano criado.</response>
    /// <response code="400">Dados inválidos (ex.: valor ou duração fora da faixa permitida).</response>
    /// <response code="409">Já existe um plano com o mesmo nome.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(PlanoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanoResponse>> Criar([FromBody] PlanoRequest req)
    {
        if (await NomeEmUso(req.Nome, null)) return Conflito($"Já existe um plano chamado '{req.Nome.Trim()}'.");

        var plano = new Plano();
        Aplicar(plano, req);
        db.Planos.Add(plano);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obter), new { id = plano.Id }, plano.ToResponse());
    }

    /// <summary>Atualiza um plano.</summary>
    /// <remarks>Alterar valor ou duração <b>não</b> afeta matrículas já realizadas — apenas as novas.</remarks>
    /// <param name="id">Identificador do plano. Exemplo: 1</param>
    /// <response code="200">Plano atualizado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Não existe plano com o id informado.</response>
    /// <response code="409">Outro plano já usa esse nome.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(PlanoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanoResponse>> Atualizar(int id, [FromBody] PlanoRequest req)
    {
        var plano = await db.Planos.FindAsync(id);
        if (plano is null) return NaoEncontrado($"Plano {id} não encontrado.");
        if (await NomeEmUso(req.Nome, id)) return Conflito($"Já existe um plano chamado '{req.Nome.Trim()}'.");

        Aplicar(plano, req);
        await db.SaveChangesAsync();
        return Ok(plano.ToResponse());
    }

    /// <summary>Exclui um plano.</summary>
    /// <remarks>Só é possível excluir planos que nunca tiveram matrículas. Para planos já utilizados, faça PUT com `"ativo": false`.</remarks>
    /// <param name="id">Identificador do plano. Exemplo: 5</param>
    /// <response code="204">Plano excluído.</response>
    /// <response code="404">Não existe plano com o id informado.</response>
    /// <response code="409">O plano possui matrículas vinculadas.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var plano = await db.Planos.FindAsync(id);
        if (plano is null) return NaoEncontrado($"Plano {id} não encontrado.");
        if (await db.Matriculas.AnyAsync(m => m.PlanoId == id))
            return Conflito($"O plano {id} possui matrículas vinculadas e não pode ser excluído. Inative-o com PUT (\"ativo\": false).");

        db.Planos.Remove(plano);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private Task<bool> NomeEmUso(string nome, int? idAtual)
    {
        var n = nome.Trim().ToLower();
        return db.Planos.AnyAsync(p => p.Nome.ToLower() == n && p.Id != idAtual);
    }

    private static void Aplicar(Plano plano, PlanoRequest req)
    {
        plano.Nome = req.Nome.Trim();
        plano.Descricao = string.IsNullOrWhiteSpace(req.Descricao) ? null : req.Descricao.Trim();
        plano.ValorMensal = Math.Round(req.ValorMensal!.Value, 2);
        plano.DuracaoMeses = req.DuracaoMeses!.Value;
        plano.Ativo = req.Ativo;
    }
}
