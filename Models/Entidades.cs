namespace AcademiaApi.Models;

/// <summary>Situação de uma matrícula.</summary>
public enum StatusMatricula
{
    /// <summary>Matrícula vigente.</summary>
    Ativa,
    /// <summary>Matrícula cancelada antes do fim.</summary>
    Cancelada,
    /// <summary>Matrícula cuja data de término já passou.</summary>
    Expirada
}

/// <summary>Situação de uma reserva de aula.</summary>
public enum StatusReserva
{
    /// <summary>Vaga garantida na aula.</summary>
    Confirmada,
    /// <summary>Reserva cancelada; a vaga foi liberada.</summary>
    Cancelada
}

/// <summary>Aluno da academia.</summary>
public class Aluno
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateOnly DataNascimento { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

/// <summary>Plano de assinatura (mensal, trimestral...).</summary>
public class Plano
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorMensal { get; set; }
    public int DuracaoMeses { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}

/// <summary>Vínculo entre um aluno e um plano durante um período.</summary>
public class Matricula
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
    public int PlanoId { get; set; }
    public Plano? Plano { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusMatricula Status { get; set; } = StatusMatricula.Ativa;
    public DateTime? DataCancelamento { get; set; }
}

/// <summary>Instrutor responsável por aulas coletivas.</summary>
public class Instrutor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Aula> Aulas { get; set; } = new List<Aula>();
}

/// <summary>Aula coletiva com horário e número limitado de vagas.</summary>
public class Aula
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Sala { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public int DuracaoMinutos { get; set; }
    public int Capacidade { get; set; }
    public int InstrutorId { get; set; }
    public Instrutor? Instrutor { get; set; }

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

/// <summary>Reserva de vaga de um aluno em uma aula.</summary>
public class Reserva
{
    public int Id { get; set; }
    public int AulaId { get; set; }
    public Aula? Aula { get; set; }
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
    public DateTime DataReserva { get; set; }
    public StatusReserva Status { get; set; } = StatusReserva.Confirmada;
    public DateTime? DataCancelamento { get; set; }
}
