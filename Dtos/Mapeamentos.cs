using AcademiaApi.Models;

namespace AcademiaApi.Dtos;

/// <summary>Conversões entidade → DTO de resposta.</summary>
public static class Mapeamentos
{
    public static PlanoResponse ToResponse(this Plano p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Descricao = p.Descricao,
        ValorMensal = p.ValorMensal,
        DuracaoMeses = p.DuracaoMeses,
        ValorTotal = p.ValorMensal * p.DuracaoMeses,
        Ativo = p.Ativo
    };

    /// <summary>Requer Aluno e Plano carregados (Include).</summary>
    public static MatriculaResponse ToResponse(this Matricula m, DateOnly hoje) => new()
    {
        Id = m.Id,
        AlunoId = m.AlunoId,
        AlunoNome = m.Aluno?.Nome ?? string.Empty,
        PlanoId = m.PlanoId,
        PlanoNome = m.Plano?.Nome ?? string.Empty,
        DataInicio = m.DataInicio,
        DataFim = m.DataFim,
        ValorTotal = m.ValorTotal,
        // "Expirada" é calculada: matrícula ativa cuja data de fim já passou.
        Status = m.Status == StatusMatricula.Ativa && m.DataFim < hoje ? StatusMatricula.Expirada : m.Status,
        DataCancelamento = m.DataCancelamento
    };

    /// <summary>Requer Aula e Aluno carregados (Include).</summary>
    public static ReservaResponse ToResponse(this Reserva r) => new()
    {
        Id = r.Id,
        AulaId = r.AulaId,
        AulaTitulo = r.Aula?.Titulo ?? string.Empty,
        AulaDataHoraInicio = r.Aula?.DataHoraInicio ?? default,
        AlunoId = r.AlunoId,
        AlunoNome = r.Aluno?.Nome ?? string.Empty,
        DataReserva = r.DataReserva,
        Status = r.Status,
        DataCancelamento = r.DataCancelamento
    };
}
