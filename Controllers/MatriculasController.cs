using AcademiaApi.Data;
using AcademiaApi.Dtos;
using AcademiaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Controllers;

/// <summary>
/// REGRA DE NEGÓCIO — Matrícula de alunos em planos. A API calcula a data de término e o valor total,
/// impede matrículas duplicadas e permite cancelar uma matrícula (liberando as reservas futuras do aluno).
/// </summary>
[Route("api/matriculas")]
public class MatriculasController(AcademiaContext db) : ApiControllerBase
{
    /// <summary>Lista as matrículas.</summary>
    /// <param name="alunoId">Filtra pelo aluno.</param>
    /// <param name="status">Filtra pela situação: Ativa, Cancelada ou Expirada.</param>
    /// <response code="200">Lista de matrículas, da mais recente para a mais antiga.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MatriculaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MatriculaResponse>>> Listar([FromQuery] int? alunoId, [FromQuery] StatusMatricula? status)
    {
        var query = db.Matriculas.AsNoTracking().Include(m => m.Aluno).Include(m => m.Plano).AsQueryable();
        if (alunoId.HasValue) query = query.Where(m => m.AlunoId == alunoId.Value);

        var lista = (await query.OrderByDescending(m => m.DataInicio).ThenByDescending(m => m.Id).ToListAsync())
            .Select(m => m.ToResponse(Hoje));
        if (status.HasValue) lista = lista.Where(m => m.Status == status.Value);
        return Ok(lista);
    }

    /// <summary>Consulta uma matrícula pelo id.</summary>
    /// <param name="id">Identificador da matrícula. Exemplo: 1</param>
    /// <response code="200">Matrícula encontrada.</response>
    /// <response code="404">Não existe matrícula com o id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MatriculaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatriculaResponse>> Obter(int id)
    {
        var m = await db.Matriculas.AsNoTracking().Include(x => x.Aluno).Include(x => x.Plano).FirstOrDefaultAsync(x => x.Id == id);
        return m is null ? NaoEncontrado($"Matrícula {id} não encontrada.") : Ok(m.ToResponse(Hoje));
    }

    /// <summary>Matricula um aluno em um plano (regra de negócio).</summary>
    /// <remarks>
    /// O que a API faz:
    /// 1. Verifica se o aluno existe e está **ativo**;
    /// 2. Verifica se o plano existe e está **ativo**;
    /// 3. Garante que o aluno **não tenha outra matrícula vigente**;
    /// 4. Define `dataInicio` (hoje, se omitida — não pode ser no passado);
    /// 5. Calcula `dataFim` = dataInicio + duração do plano − 1 dia;
    /// 6. Calcula `valorTotal` = valorMensal × duracaoMeses.
    ///
    /// Exemplo (a aluna 5 — Elisa — não tem matrícula nos dados de exemplo):
    ///
    ///     {
    ///       "alunoId": 5,
    ///       "planoId": 2
    ///     }
    /// </remarks>
    /// <response code="201">Matrícula criada com datas e valor calculados.</response>
    /// <response code="400">Dados inválidos, aluno inativo, plano inativo ou data de início no passado.</response>
    /// <response code="404">Aluno ou plano não encontrado.</response>
    /// <response code="409">O aluno já possui matrícula vigente.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MatriculaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MatriculaResponse>> Matricular([FromBody] MatriculaRequest req)
    {
        var aluno = await db.Alunos.FindAsync(req.AlunoId);
        if (aluno is null) return NaoEncontrado($"Aluno {req.AlunoId} não encontrado.");
        if (!aluno.Ativo) return RequisicaoInvalida($"O aluno {aluno.Id} ({aluno.Nome}) está inativo e não pode ser matriculado.");

        var plano = await db.Planos.FindAsync(req.PlanoId);
        if (plano is null) return NaoEncontrado($"Plano {req.PlanoId} não encontrado.");
        if (!plano.Ativo) return RequisicaoInvalida($"O plano '{plano.Nome}' está inativo e não aceita novas matrículas.");

        var hoje = Hoje;
        var inicio = req.DataInicio ?? hoje;
        if (inicio < hoje) return RequisicaoInvalida("A data de início não pode estar no passado.");

        var vigente = await db.Matriculas.FirstOrDefaultAsync(m =>
            m.AlunoId == aluno.Id && m.Status == StatusMatricula.Ativa && m.DataFim >= hoje);
        if (vigente is not null)
            return Conflito($"O aluno {aluno.Id} já possui a matrícula {vigente.Id} vigente até {vigente.DataFim:dd/MM/yyyy}. Cancele-a antes de criar outra.");

        var matricula = new Matricula
        {
            AlunoId = aluno.Id,
            PlanoId = plano.Id,
            DataInicio = inicio,
            DataFim = inicio.AddMonths(plano.DuracaoMeses).AddDays(-1),
            ValorTotal = plano.ValorMensal * plano.DuracaoMeses,
            Status = StatusMatricula.Ativa
        };
        db.Matriculas.Add(matricula);
        await db.SaveChangesAsync();

        matricula.Aluno = aluno;
        matricula.Plano = plano;
        return CreatedAtAction(nameof(Obter), new { id = matricula.Id }, matricula.ToResponse(hoje));
    }

    /// <summary>Cancela uma matrícula (regra de negócio).</summary>
    /// <remarks>
    /// Muda a situação para `Cancelada`, registra a data do cancelamento e **cancela automaticamente
    /// todas as reservas futuras confirmadas** do aluno, liberando as vagas nas aulas.
    /// Não requer corpo.
    /// </remarks>
    /// <param name="id">Identificador da matrícula. Exemplo: 2</param>
    /// <response code="200">Matrícula cancelada.</response>
    /// <response code="404">Não existe matrícula com o id informado.</response>
    /// <response code="409">A matrícula já está cancelada ou expirada.</response>
    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(MatriculaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MatriculaResponse>> Cancelar(int id)
    {
        var m = await db.Matriculas.Include(x => x.Aluno).Include(x => x.Plano).FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return NaoEncontrado($"Matrícula {id} não encontrada.");

        var hoje = Hoje;
        var statusAtual = m.ToResponse(hoje).Status;
        if (statusAtual != StatusMatricula.Ativa)
            return Conflito($"A matrícula {id} não pode ser cancelada porque está '{statusAtual}'.");

        var agora = Agora;
        m.Status = StatusMatricula.Cancelada;
        m.DataCancelamento = agora;

        var reservasFuturas = await db.Reservas
            .Where(r => r.AlunoId == m.AlunoId && r.Status == StatusReserva.Confirmada && r.Aula!.DataHoraInicio > agora)
            .ToListAsync();
        foreach (var r in reservasFuturas)
        {
            r.Status = StatusReserva.Cancelada;
            r.DataCancelamento = agora;
        }

        await db.SaveChangesAsync();
        return Ok(m.ToResponse(hoje));
    }
}
