using System.Linq.Expressions;
using AcademiaApi.Data;
using AcademiaApi.Dtos;
using AcademiaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Controllers;

/// <summary>
/// Instrutores responsáveis pelas aulas coletivas (CRUD).
/// </summary>
[Route("api/instrutores")]
public class InstrutoresController(AcademiaContext db) : ApiControllerBase
{
    private static readonly Expression<Func<Instrutor, InstrutorResponse>> Projecao = i => new InstrutorResponse
    {
        Id = i.Id,
        Nome = i.Nome,
        Especialidade = i.Especialidade,
        Email = i.Email,
        QuantidadeAulas = i.Aulas.Count
    };

    /// <summary>Lista os instrutores.</summary>
    /// <param name="especialidade">Filtra pela especialidade (texto parcial, sem diferenciar maiúsculas). Exemplo: yoga</param>
    /// <response code="200">Lista de instrutores ordenada por nome.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InstrutorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InstrutorResponse>>> Listar([FromQuery] string? especialidade)
    {
        var query = db.Instrutores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(especialidade))
            query = query.Where(i => i.Especialidade.ToLower().Contains(especialidade.Trim().ToLower()));
        return Ok(await query.OrderBy(i => i.Nome).Select(Projecao).ToListAsync());
    }

    /// <summary>Consulta um instrutor pelo id.</summary>
    /// <param name="id">Identificador do instrutor. Exemplo: 1</param>
    /// <response code="200">Instrutor encontrado.</response>
    /// <response code="404">Não existe instrutor com o id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(InstrutorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstrutorResponse>> Obter(int id)
    {
        var instrutor = await db.Instrutores.AsNoTracking().Where(i => i.Id == id).Select(Projecao).FirstOrDefaultAsync();
        return instrutor is null ? NaoEncontrado($"Instrutor {id} não encontrado.") : Ok(instrutor);
    }

    /// <summary>Cadastra um novo instrutor.</summary>
    /// <remarks>
    /// Exemplo de corpo:
    ///
    ///     {
    ///       "nome": "Patrícia Gomes",
    ///       "especialidade": "Pilates",
    ///       "email": "patricia.gomes@academia.exemplo.com"
    ///     }
    /// </remarks>
    /// <response code="201">Instrutor criado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Já existe instrutor com o mesmo e-mail.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(InstrutorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InstrutorResponse>> Criar([FromBody] InstrutorRequest req)
    {
        var email = req.Email.Trim().ToLower();
        if (await db.Instrutores.AnyAsync(i => i.Email == email))
            return Conflito($"Já existe um instrutor com o e-mail {email}.");

        var instrutor = new Instrutor { Nome = req.Nome.Trim(), Especialidade = req.Especialidade.Trim(), Email = email };
        db.Instrutores.Add(instrutor);
        await db.SaveChangesAsync();

        var resp = await db.Instrutores.AsNoTracking().Where(i => i.Id == instrutor.Id).Select(Projecao).FirstAsync();
        return CreatedAtAction(nameof(Obter), new { id = instrutor.Id }, resp);
    }

    /// <summary>Atualiza um instrutor.</summary>
    /// <param name="id">Identificador do instrutor. Exemplo: 1</param>
    /// <response code="200">Instrutor atualizado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Não existe instrutor com o id informado.</response>
    /// <response code="409">O e-mail já pertence a outro instrutor.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(InstrutorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InstrutorResponse>> Atualizar(int id, [FromBody] InstrutorRequest req)
    {
        var instrutor = await db.Instrutores.FindAsync(id);
        if (instrutor is null) return NaoEncontrado($"Instrutor {id} não encontrado.");

        var email = req.Email.Trim().ToLower();
        if (await db.Instrutores.AnyAsync(i => i.Email == email && i.Id != id))
            return Conflito($"Já existe um instrutor com o e-mail {email}.");

        instrutor.Nome = req.Nome.Trim();
        instrutor.Especialidade = req.Especialidade.Trim();
        instrutor.Email = email;
        await db.SaveChangesAsync();

        return Ok(await db.Instrutores.AsNoTracking().Where(i => i.Id == id).Select(Projecao).FirstAsync());
    }

    /// <summary>Exclui um instrutor.</summary>
    /// <remarks>Só é possível excluir instrutores sem aulas atribuídas. Reatribua ou exclua as aulas antes.</remarks>
    /// <param name="id">Identificador do instrutor.</param>
    /// <response code="204">Instrutor excluído.</response>
    /// <response code="404">Não existe instrutor com o id informado.</response>
    /// <response code="409">O instrutor possui aulas vinculadas.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var instrutor = await db.Instrutores.FindAsync(id);
        if (instrutor is null) return NaoEncontrado($"Instrutor {id} não encontrado.");
        if (await db.Aulas.AnyAsync(a => a.InstrutorId == id))
            return Conflito($"O instrutor {id} possui aulas vinculadas e não pode ser excluído.");

        db.Instrutores.Remove(instrutor);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
