using System.Linq.Expressions;
using AcademiaApi.Data;
using AcademiaApi.Dtos;
using AcademiaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Controllers;

/// <summary>
/// Cadastro de alunos da academia (CRUD) e consulta do histórico de matrículas e reservas de cada aluno.
/// </summary>
[Route("api/alunos")]
public class AlunosController(AcademiaContext db) : ApiControllerBase
{
    private static Expression<Func<Aluno, AlunoResponse>> Projecao(DateOnly hoje) => a => new AlunoResponse
    {
        Id = a.Id,
        Nome = a.Nome,
        Cpf = a.Cpf,
        Email = a.Email,
        Telefone = a.Telefone,
        DataNascimento = a.DataNascimento,
        DataCadastro = a.DataCadastro,
        Ativo = a.Ativo,
        PossuiMatriculaAtiva = a.Matriculas.Any(m => m.Status == StatusMatricula.Ativa && m.DataFim >= hoje)
    };

    /// <summary>Lista os alunos cadastrados.</summary>
    /// <remarks>
    /// Retorna todos os alunos ordenados por nome. Os filtros são opcionais e podem ser combinados.
    ///
    /// Exemplo: `GET /api/alunos?nome=ana&amp;ativo=true`
    /// </remarks>
    /// <param name="nome">Filtra alunos cujo nome contém o texto informado (sem diferenciar maiúsculas/minúsculas).</param>
    /// <param name="ativo">Filtra por situação: true = somente ativos, false = somente inativos.</param>
    /// <response code="200">Lista de alunos (pode ser vazia).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlunoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlunoResponse>>> Listar([FromQuery] string? nome, [FromQuery] bool? ativo)
    {
        var query = db.Alunos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(a => a.Nome.ToLower().Contains(nome.Trim().ToLower()));
        if (ativo.HasValue)
            query = query.Where(a => a.Ativo == ativo.Value);

        return Ok(await query.OrderBy(a => a.Nome).Select(Projecao(Hoje)).ToListAsync());
    }

    /// <summary>Consulta um aluno pelo id.</summary>
    /// <param name="id">Identificador do aluno. Exemplo: 1</param>
    /// <response code="200">Aluno encontrado.</response>
    /// <response code="404">Não existe aluno com o id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AlunoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlunoResponse>> Obter(int id)
    {
        var aluno = await db.Alunos.AsNoTracking().Where(a => a.Id == id).Select(Projecao(Hoje)).FirstOrDefaultAsync();
        return aluno is null ? NaoEncontrado($"Aluno {id} não encontrado.") : Ok(aluno);
    }

    /// <summary>Cadastra um novo aluno.</summary>
    /// <remarks>
    /// Regras:
    /// - `cpf`: 11 dígitos numéricos, único no sistema;
    /// - `email`: formato válido, único no sistema;
    /// - `dataNascimento`: aluno com idade entre 14 e 100 anos.
    ///
    /// Exemplo de corpo:
    ///
    ///     {
    ///       "nome": "Gabriel Martins",
    ///       "cpf": "11122233307",
    ///       "email": "gabriel.martins@exemplo.com",
    ///       "telefone": "(11) 97777-1234",
    ///       "dataNascimento": "2001-04-18",
    ///       "ativo": true
    ///     }
    /// </remarks>
    /// <response code="201">Aluno criado. O cabeçalho `Location` contém a URL do novo recurso.</response>
    /// <response code="400">Dados inválidos (campo obrigatório ausente, CPF/e-mail mal formatado, idade fora da faixa).</response>
    /// <response code="409">Já existe aluno com o mesmo CPF ou e-mail.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AlunoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlunoResponse>> Criar([FromBody] AlunoRequest req)
    {
        var erro = await ValidarAsync(req, idAtual: null);
        if (erro is not null) return erro;

        var aluno = new Aluno { DataCadastro = Agora };
        Aplicar(aluno, req);
        db.Alunos.Add(aluno);
        await db.SaveChangesAsync();

        var resp = await db.Alunos.AsNoTracking().Where(a => a.Id == aluno.Id).Select(Projecao(Hoje)).FirstAsync();
        return CreatedAtAction(nameof(Obter), new { id = aluno.Id }, resp);
    }

    /// <summary>Atualiza todos os dados de um aluno.</summary>
    /// <remarks>
    /// Substitui os dados do aluno pelos enviados no corpo (todos os campos obrigatórios devem ser informados).
    /// As mesmas validações do cadastro se aplicam. Use `"ativo": false` para inativar o aluno.
    /// </remarks>
    /// <param name="id">Identificador do aluno a ser atualizado. Exemplo: 1</param>
    /// <response code="200">Aluno atualizado; retorna os dados atuais.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Não existe aluno com o id informado.</response>
    /// <response code="409">CPF ou e-mail já pertencem a outro aluno.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(AlunoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlunoResponse>> Atualizar(int id, [FromBody] AlunoRequest req)
    {
        var aluno = await db.Alunos.FindAsync(id);
        if (aluno is null) return NaoEncontrado($"Aluno {id} não encontrado.");

        var erro = await ValidarAsync(req, idAtual: id);
        if (erro is not null) return erro;

        Aplicar(aluno, req);
        await db.SaveChangesAsync();

        return Ok(await db.Alunos.AsNoTracking().Where(a => a.Id == id).Select(Projecao(Hoje)).FirstAsync());
    }

    /// <summary>Exclui um aluno.</summary>
    /// <remarks>Remove também, em cascata, as matrículas e reservas do aluno. A operação não pode ser desfeita.</remarks>
    /// <param name="id">Identificador do aluno. Exemplo: 6</param>
    /// <response code="204">Aluno excluído (sem corpo de resposta).</response>
    /// <response code="404">Não existe aluno com o id informado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var aluno = await db.Alunos.FindAsync(id);
        if (aluno is null) return NaoEncontrado($"Aluno {id} não encontrado.");

        db.Alunos.Remove(aluno);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Lista o histórico de matrículas de um aluno.</summary>
    /// <param name="id">Identificador do aluno. Exemplo: 1</param>
    /// <response code="200">Matrículas do aluno, da mais recente para a mais antiga.</response>
    /// <response code="404">Não existe aluno com o id informado.</response>
    [HttpGet("{id:int}/matriculas")]
    [ProducesResponseType(typeof(IEnumerable<MatriculaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MatriculaResponse>>> Matriculas(int id)
    {
        if (!await db.Alunos.AnyAsync(a => a.Id == id)) return NaoEncontrado($"Aluno {id} não encontrado.");

        var lista = await db.Matriculas.AsNoTracking().Include(m => m.Aluno).Include(m => m.Plano)
            .Where(m => m.AlunoId == id).OrderByDescending(m => m.DataInicio).ToListAsync();
        return Ok(lista.Select(m => m.ToResponse(Hoje)));
    }

    /// <summary>Lista as reservas de aulas de um aluno.</summary>
    /// <param name="id">Identificador do aluno. Exemplo: 1</param>
    /// <response code="200">Reservas do aluno (confirmadas e canceladas), ordenadas pela data da aula.</response>
    /// <response code="404">Não existe aluno com o id informado.</response>
    [HttpGet("{id:int}/reservas")]
    [ProducesResponseType(typeof(IEnumerable<ReservaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ReservaResponse>>> Reservas(int id)
    {
        if (!await db.Alunos.AnyAsync(a => a.Id == id)) return NaoEncontrado($"Aluno {id} não encontrado.");

        var lista = await db.Reservas.AsNoTracking().Include(r => r.Aula).Include(r => r.Aluno)
            .Where(r => r.AlunoId == id).OrderBy(r => r.Aula!.DataHoraInicio).ToListAsync();
        return Ok(lista.Select(r => r.ToResponse()));
    }

    // ------------------------------------------------------------------

    private async Task<ActionResult?> ValidarAsync(AlunoRequest req, int? idAtual)
    {
        var nasc = req.DataNascimento!.Value;
        var idade = Hoje.Year - nasc.Year - (Hoje < nasc.AddYears(Hoje.Year - nasc.Year) ? 1 : 0);
        if (idade < 14 || idade > 100)
            return RequisicaoInvalida($"A idade do aluno deve estar entre 14 e 100 anos (idade calculada: {idade}).");

        var email = req.Email.Trim().ToLower();
        if (await db.Alunos.AnyAsync(a => a.Cpf == req.Cpf && a.Id != idAtual))
            return Conflito($"Já existe um aluno cadastrado com o CPF {req.Cpf}.");
        if (await db.Alunos.AnyAsync(a => a.Email == email && a.Id != idAtual))
            return Conflito($"Já existe um aluno cadastrado com o e-mail {email}.");
        return null;
    }

    private static void Aplicar(Aluno aluno, AlunoRequest req)
    {
        aluno.Nome = req.Nome.Trim();
        aluno.Cpf = req.Cpf;
        aluno.Email = req.Email.Trim().ToLower();
        aluno.Telefone = string.IsNullOrWhiteSpace(req.Telefone) ? null : req.Telefone.Trim();
        aluno.DataNascimento = req.DataNascimento!.Value;
        aluno.Ativo = req.Ativo;
    }
}
