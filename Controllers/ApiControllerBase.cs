using Microsoft.AspNetCore.Mvc;

namespace AcademiaApi.Controllers;

/// <summary>
/// Base comum: todas as respostas de erro seguem o padrão ProblemDetails (RFC 9457),
/// com os campos title, status e detail.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Data/hora local sem fuso (mesmo formato em que é lida do banco).</summary>
    protected static DateTime Agora => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    protected static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Now);

    /// <summary>404 — o recurso (da rota ou referenciado no corpo) não existe.</summary>
    protected ObjectResult NaoEncontrado(string detalhe) =>
        Problem(detail: detalhe, statusCode: StatusCodes.Status404NotFound, title: "Recurso não encontrado");

    /// <summary>400 — dados válidos no formato, mas que violam uma regra (ex.: data no passado).</summary>
    protected ObjectResult RequisicaoInvalida(string detalhe) =>
        Problem(detail: detalhe, statusCode: StatusCodes.Status400BadRequest, title: "Requisição inválida");

    /// <summary>409 — conflito com o estado atual (duplicidade, aula lotada, vínculo existente...).</summary>
    protected ObjectResult Conflito(string detalhe) =>
        Problem(detail: detalhe, statusCode: StatusCodes.Status409Conflict, title: "Conflito com regra de negócio");
}
