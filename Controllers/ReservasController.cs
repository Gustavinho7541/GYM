using AcademiaApi.Data;
using AcademiaApi.Dtos;
using AcademiaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Controllers;

/// <summary>
/// REGRA DE NEGÓCIO — Reserva de vagas em aulas coletivas. A API controla a lotação,
/// exige matrícula vigente, impede reservas duplicadas e permite cancelar liberando a vaga.
/// </summary>
[Route("api/reservas")]
public class ReservasController(AcademiaContext db) : ApiControllerBase
{
    /// <summary>Lista as reservas.</summary>
    /// <param name="alunoId">Filtra pelo aluno.</param>
    /// <param name="aulaId">Filtra pela aula.</param>
    /// <param name="status">Filtra pela situação: Confirmada ou Cancelada.</param>
    /// <response code="200">Lista de reservas, das mais recentes para as mais antigas.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReservaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservaResponse>>> Listar([FromQuery] int? alunoId, [FromQuery] int? aulaId, [FromQuery] StatusReserva? status)
    {
        var query = db.Reservas.AsNoTracking().Include(r => r.Aula).Include(r => r.Aluno).AsQueryable();
        if (alunoId.HasValue) query = query.Where(r => r.AlunoId == alunoId.Value);
        if (aulaId.HasValue) query = query.Where(r => r.AulaId == aulaId.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        var lista = await query.OrderByDescending(r => r.DataReserva).ToListAsync();
        return Ok(lista.Select(r => r.ToResponse()));
    }

    /// <summary>Consulta uma reserva pelo id.</summary>
    /// <param name="id">Identificador da reserva. Exemplo: 1</param>
    /// <response code="200">Reserva encontrada.</response>
    /// <response code="404">Não existe reserva com o id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservaResponse>> Obter(int id)
    {
        var r = await db.Reservas.AsNoTracking().Include(x => x.Aula).Include(x => x.Aluno).FirstOrDefaultAsync(x => x.Id == id);
        return r is null ? NaoEncontrado($"Reserva {id} não encontrada.") : Ok(r.ToResponse());
    }

    /// <summary>Reserva uma vaga em uma aula (regra de negócio).</summary>
    /// <remarks>
    /// O que a API verifica, nesta ordem:
    /// 1. A aula e o aluno existem (senão **404**);
    /// 2. O aluno está ativo e tem **matrícula vigente** (senão **400**);
    /// 3. A aula ainda **não começou** (senão **400**);
    /// 4. O aluno **ainda não reservou** essa aula (senão **409**);
    /// 5. A aula **tem vaga** — reservas confirmadas &lt; capacidade (senão **409**).
    ///
    /// Exemplo de sucesso (Carla reserva o Spinning Intenso):
    ///
    ///     { "aulaId": 1, "alunoId": 3 }
    ///
    /// Exemplo de aula lotada (aula 3 tem capacidade 2 e já tem 2 reservas) → 409:
    ///
    ///     { "aulaId": 3, "alunoId": 3 }
    ///
    /// Exemplo de aluno sem matrícula (Elisa) → 400:
    ///
    ///     { "aulaId": 1, "alunoId": 5 }
    /// </remarks>
    /// <response code="201">Reserva confirmada.</response>
    /// <response code="400">Dados inválidos, aluno inativo/sem matrícula vigente ou aula já iniciada.</response>
    /// <response code="404">Aula ou aluno não encontrado.</response>
    /// <response code="409">Aluno já reservou essa aula ou a aula está lotada.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResponse>> Reservar([FromBody] ReservaRequest req)
    {
        var aula = await db.Aulas.FindAsync(req.AulaId);
        if (aula is null) return NaoEncontrado($"Aula {req.AulaId} não encontrada.");
        var aluno = await db.Alunos.FindAsync(req.AlunoId);
        if (aluno is null) return NaoEncontrado($"Aluno {req.AlunoId} não encontrado.");

        if (!aluno.Ativo) return RequisicaoInvalida($"O aluno {aluno.Id} ({aluno.Nome}) está inativo.");

        var hoje = Hoje;
        var temMatricula = await db.Matriculas.AnyAsync(m =>
            m.AlunoId == aluno.Id && m.Status == StatusMatricula.Ativa && m.DataInicio <= hoje && m.DataFim >= hoje);
        if (!temMatricula)
            return RequisicaoInvalida($"O aluno {aluno.Id} ({aluno.Nome}) não possui matrícula vigente. Matricule-o em POST /api/matriculas.");

        var agora = Agora;
        if (aula.DataHoraInicio <= agora)
            return RequisicaoInvalida($"A aula {aula.Id} já começou ou foi realizada em {aula.DataHoraInicio:dd/MM/yyyy HH:mm}.");

        if (await db.Reservas.AnyAsync(r => r.AulaId == aula.Id && r.AlunoId == aluno.Id && r.Status == StatusReserva.Confirmada))
            return Conflito($"O aluno {aluno.Id} já possui reserva confirmada na aula {aula.Id}.");

        var ocupadas = await db.Reservas.CountAsync(r => r.AulaId == aula.Id && r.Status == StatusReserva.Confirmada);
        if (ocupadas >= aula.Capacidade)
            return Conflito($"A aula '{aula.Titulo}' está lotada ({ocupadas}/{aula.Capacidade} vagas ocupadas).");

        var reserva = new Reserva { AulaId = aula.Id, AlunoId = aluno.Id, DataReserva = agora, Status = StatusReserva.Confirmada };
        db.Reservas.Add(reserva);
        await db.SaveChangesAsync();

        reserva.Aula = aula;
        reserva.Aluno = aluno;
        return CreatedAtAction(nameof(Obter), new { id = reserva.Id }, reserva.ToResponse());
    }

    /// <summary>Cancela uma reserva, liberando a vaga (regra de negócio).</summary>
    /// <remarks>Só reservas `Confirmada` de aulas que ainda não começaram podem ser canceladas. Não requer corpo.</remarks>
    /// <param name="id">Identificador da reserva. Exemplo: 1</param>
    /// <response code="200">Reserva cancelada; a vaga volta a ficar disponível.</response>
    /// <response code="400">A aula já começou.</response>
    /// <response code="404">Não existe reserva com o id informado.</response>
    /// <response code="409">A reserva já estava cancelada.</response>
    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservaResponse>> Cancelar(int id)
    {
        var r = await db.Reservas.Include(x => x.Aula).Include(x => x.Aluno).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NaoEncontrado($"Reserva {id} não encontrada.");
        if (r.Status == StatusReserva.Cancelada) return Conflito($"A reserva {id} já está cancelada.");

        var agora = Agora;
        if (r.Aula!.DataHoraInicio <= agora)
            return RequisicaoInvalida("Não é possível cancelar a reserva de uma aula que já começou.");

        r.Status = StatusReserva.Cancelada;
        r.DataCancelamento = agora;
        await db.SaveChangesAsync();
        return Ok(r.ToResponse());
    }
}
