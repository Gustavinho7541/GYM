using System.Linq.Expressions;
using AcademiaApi.Data;
using AcademiaApi.Dtos;
using AcademiaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Controllers;

/// <summary>
/// Grade de aulas coletivas (CRUD), com controle de vagas e consulta da lista de presença (reservas) de cada aula.
/// </summary>
[Route("api/aulas")]
public class AulasController(AcademiaContext db) : ApiControllerBase
{
    private static readonly Expression<Func<Aula, AulaResponse>> Projecao = a => new AulaResponse
    {
        Id = a.Id,
        Titulo = a.Titulo,
        Sala = a.Sala,
        DataHoraInicio = a.DataHoraInicio,
        DuracaoMinutos = a.DuracaoMinutos,
        Capacidade = a.Capacidade,
        VagasOcupadas = a.Reservas.Count(r => r.Status == StatusReserva.Confirmada),
        VagasDisponiveis = a.Capacidade - a.Reservas.Count(r => r.Status == StatusReserva.Confirmada),
        InstrutorId = a.InstrutorId,
        InstrutorNome = a.Instrutor!.Nome
    };

    /// <summary>Lista as aulas da grade.</summary>
    /// <remarks>Todos os filtros são opcionais. Exemplo: `GET /api/aulas?data=2027-06-07&amp;comVagas=true`</remarks>
    /// <param name="data">Retorna apenas aulas desse dia (AAAA-MM-DD). Exemplo: 2027-06-07</param>
    /// <param name="instrutorId">Retorna apenas aulas desse instrutor.</param>
    /// <param name="comVagas">Se true, retorna apenas aulas que ainda têm vagas disponíveis.</param>
    /// <response code="200">Lista de aulas ordenada por data/hora.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AulaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AulaResponse>>> Listar([FromQuery] DateOnly? data, [FromQuery] int? instrutorId, [FromQuery] bool comVagas = false)
    {
        var query = db.Aulas.AsNoTracking();
        if (data.HasValue)
        {
            var ini = data.Value.ToDateTime(TimeOnly.MinValue);
            var fim = ini.AddDays(1);
            query = query.Where(a => a.DataHoraInicio >= ini && a.DataHoraInicio < fim);
        }
        if (instrutorId.HasValue) query = query.Where(a => a.InstrutorId == instrutorId.Value);
        if (comVagas) query = query.Where(a => a.Reservas.Count(r => r.Status == StatusReserva.Confirmada) < a.Capacidade);

        return Ok(await query.OrderBy(a => a.DataHoraInicio).Select(Projecao).ToListAsync());
    }

    /// <summary>Consulta uma aula pelo id, incluindo a ocupação de vagas.</summary>
    /// <param name="id">Identificador da aula. Exemplo: 3</param>
    /// <response code="200">Aula encontrada.</response>
    /// <response code="404">Não existe aula com o id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AulaResponse>> Obter(int id)
    {
        var aula = await db.Aulas.AsNoTracking().Where(a => a.Id == id).Select(Projecao).FirstOrDefaultAsync();
        return aula is null ? NaoEncontrado($"Aula {id} não encontrada.") : Ok(aula);
    }

    /// <summary>Cadastra uma nova aula.</summary>
    /// <remarks>
    /// Regras:
    /// - `dataHoraInicio` deve estar no futuro;
    /// - `instrutorId` deve existir (consulte GET /api/instrutores);
    /// - o instrutor não pode ter outra aula no mesmo horário (sobreposição).
    ///
    /// Exemplo de corpo:
    ///
    ///     {
    ///       "titulo": "Pilates Solo",
    ///       "sala": "Sala Zen",
    ///       "dataHoraInicio": "2027-06-10T08:00:00",
    ///       "duracaoMinutos": 50,
    ///       "capacidade": 12,
    ///       "instrutorId": 2
    ///     }
    /// </remarks>
    /// <response code="201">Aula criada.</response>
    /// <response code="400">Dados inválidos (ex.: data no passado, capacidade fora da faixa).</response>
    /// <response code="404">O instrutor informado não existe.</response>
    /// <response code="409">O instrutor já tem outra aula nesse horário.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AulaResponse>> Criar([FromBody] AulaRequest req)
    {
        var erro = await ValidarAsync(req, idAtual: null, reservasConfirmadas: 0);
        if (erro is not null) return erro;

        var aula = new Aula();
        Aplicar(aula, req);
        db.Aulas.Add(aula);
        await db.SaveChangesAsync();

        var resp = await db.Aulas.AsNoTracking().Where(a => a.Id == aula.Id).Select(Projecao).FirstAsync();
        return CreatedAtAction(nameof(Obter), new { id = aula.Id }, resp);
    }

    /// <summary>Atualiza uma aula.</summary>
    /// <remarks>
    /// Mesmas regras do cadastro. Além disso, a nova `capacidade` não pode ser menor que o número de reservas já confirmadas.
    /// </remarks>
    /// <param name="id">Identificador da aula. Exemplo: 1</param>
    /// <response code="200">Aula atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Aula ou instrutor não encontrado.</response>
    /// <response code="409">Conflito de horário do instrutor ou capacidade menor que as reservas confirmadas.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AulaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AulaResponse>> Atualizar(int id, [FromBody] AulaRequest req)
    {
        var aula = await db.Aulas.FindAsync(id);
        if (aula is null) return NaoEncontrado($"Aula {id} não encontrada.");

        var confirmadas = await db.Reservas.CountAsync(r => r.AulaId == id && r.Status == StatusReserva.Confirmada);
        var erro = await ValidarAsync(req, idAtual: id, reservasConfirmadas: confirmadas);
        if (erro is not null) return erro;

        Aplicar(aula, req);
        await db.SaveChangesAsync();
        return Ok(await db.Aulas.AsNoTracking().Where(a => a.Id == id).Select(Projecao).FirstAsync());
    }

    /// <summary>Exclui uma aula.</summary>
    /// <remarks>As reservas da aula também são excluídas (cascata).</remarks>
    /// <param name="id">Identificador da aula. Exemplo: 4</param>
    /// <response code="204">Aula excluída.</response>
    /// <response code="404">Não existe aula com o id informado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var aula = await db.Aulas.FindAsync(id);
        if (aula is null) return NaoEncontrado($"Aula {id} não encontrada.");

        db.Aulas.Remove(aula);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Lista as reservas (lista de presença) de uma aula.</summary>
    /// <param name="id">Identificador da aula. Exemplo: 3</param>
    /// <param name="somenteConfirmadas">Se true (padrão), omite reservas canceladas.</param>
    /// <response code="200">Reservas da aula.</response>
    /// <response code="404">Não existe aula com o id informado.</response>
    [HttpGet("{id:int}/reservas")]
    [ProducesResponseType(typeof(IEnumerable<ReservaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ReservaResponse>>> Reservas(int id, [FromQuery] bool somenteConfirmadas = true)
    {
        if (!await db.Aulas.AnyAsync(a => a.Id == id)) return NaoEncontrado($"Aula {id} não encontrada.");

        var query = db.Reservas.AsNoTracking().Include(r => r.Aula).Include(r => r.Aluno).Where(r => r.AulaId == id);
        if (somenteConfirmadas) query = query.Where(r => r.Status == StatusReserva.Confirmada);
        var lista = await query.OrderBy(r => r.DataReserva).ToListAsync();
        return Ok(lista.Select(r => r.ToResponse()));
    }

    // ------------------------------------------------------------------

    private async Task<ActionResult?> ValidarAsync(AulaRequest req, int? idAtual, int reservasConfirmadas)
    {
        var inicio = req.DataHoraInicio!.Value;
        if (inicio <= Agora)
            return RequisicaoInvalida("A data/hora de início da aula deve estar no futuro.");

        if (!await db.Instrutores.AnyAsync(i => i.Id == req.InstrutorId))
            return NaoEncontrado($"Instrutor {req.InstrutorId} não encontrado.");

        if (req.Capacidade < reservasConfirmadas)
            return Conflito($"A aula já possui {reservasConfirmadas} reserva(s) confirmada(s); a capacidade não pode ser menor que isso.");

        // Sobreposição de horário do mesmo instrutor (verificada em memória, entre as aulas do mesmo dia).
        var fim = inicio.AddMinutes(req.DuracaoMinutos!.Value);
        var diaIni = inicio.Date.AddDays(-1);
        var diaFim = inicio.Date.AddDays(2);
        var outras = await db.Aulas.AsNoTracking()
            .Where(a => a.InstrutorId == req.InstrutorId && a.Id != idAtual && a.DataHoraInicio >= diaIni && a.DataHoraInicio < diaFim)
            .ToListAsync();
        var choque = outras.FirstOrDefault(a => a.DataHoraInicio < fim && inicio < a.DataHoraInicio.AddMinutes(a.DuracaoMinutos));
        if (choque is not null)
            return Conflito($"O instrutor já possui a aula '{choque.Titulo}' (id {choque.Id}) em {choque.DataHoraInicio:dd/MM/yyyy HH:mm}, que se sobrepõe a esse horário.");

        return null;
    }

    private static void Aplicar(Aula aula, AulaRequest req)
    {
        aula.Titulo = req.Titulo.Trim();
        aula.Sala = string.IsNullOrWhiteSpace(req.Sala) ? null : req.Sala.Trim();
        aula.DataHoraInicio = req.DataHoraInicio!.Value;
        aula.DuracaoMinutos = req.DuracaoMinutos!.Value;
        aula.Capacidade = req.Capacidade!.Value;
        aula.InstrutorId = req.InstrutorId!.Value;
    }
}
