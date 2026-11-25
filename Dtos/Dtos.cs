using System.ComponentModel.DataAnnotations;
using AcademiaApi.Models;

namespace AcademiaApi.Dtos;

// =====================================================================
//  ALUNOS
// =====================================================================

/// <summary>Dados para cadastrar ou atualizar um aluno.</summary>
public class AlunoRequest
{
    /// <summary>Nome completo do aluno (3 a 100 caracteres).</summary>
    /// <example>Gabriel Martins</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF com exatamente 11 dígitos, somente números. Deve ser único.</summary>
    /// <example>11122233307</example>
    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 dígitos numéricos, sem pontos ou traço.")]
    public string Cpf { get; set; } = string.Empty;

    /// <summary>E-mail válido. Deve ser único.</summary>
    /// <example>gabriel.martins@exemplo.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Telefone de contato (opcional).</summary>
    /// <example>(11) 97777-1234</example>
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    public string? Telefone { get; set; }

    /// <summary>Data de nascimento no formato AAAA-MM-DD. O aluno deve ter entre 14 e 100 anos.</summary>
    /// <example>2001-04-18</example>
    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    public DateOnly? DataNascimento { get; set; }

    /// <summary>Indica se o aluno está ativo. Alunos inativos não podem se matricular nem reservar aulas. Padrão: true.</summary>
    /// <example>true</example>
    public bool Ativo { get; set; } = true;
}

/// <summary>Aluno retornado pela API.</summary>
public class AlunoResponse
{
    /// <example>1</example>
    public int Id { get; set; }
    /// <example>Ana Souza</example>
    public string Nome { get; set; } = string.Empty;
    /// <example>11122233301</example>
    public string Cpf { get; set; } = string.Empty;
    /// <example>ana.souza@exemplo.com</example>
    public string Email { get; set; } = string.Empty;
    /// <example>(11) 98888-0001</example>
    public string? Telefone { get; set; }
    /// <example>1995-03-12</example>
    public DateOnly DataNascimento { get; set; }
    /// <summary>Data/hora em que o aluno foi cadastrado (gerada pelo servidor).</summary>
    /// <example>2026-01-10T09:00:00</example>
    public DateTime DataCadastro { get; set; }
    /// <example>true</example>
    public bool Ativo { get; set; }
    /// <summary>Indica se o aluno possui matrícula ativa e vigente hoje.</summary>
    /// <example>true</example>
    public bool PossuiMatriculaAtiva { get; set; }
}

// =====================================================================
//  PLANOS
// =====================================================================

/// <summary>Dados para cadastrar ou atualizar um plano.</summary>
public class PlanoRequest
{
    /// <summary>Nome do plano (3 a 60 caracteres).</summary>
    /// <example>Bimestral</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 60 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Descrição do que o plano oferece (opcional, até 300 caracteres).</summary>
    /// <example>2 meses de acesso livre à musculação.</example>
    [StringLength(300)]
    public string? Descricao { get; set; }

    /// <summary>Valor cobrado por mês, em reais. Entre 1 e 10000.</summary>
    /// <example>119.90</example>
    [Required(ErrorMessage = "O valor mensal é obrigatório.")]
    [Range(typeof(decimal), "1", "10000", ErrorMessage = "O valor mensal deve estar entre 1 e 10000.")]
    public decimal? ValorMensal { get; set; }

    /// <summary>Duração do plano em meses (1 a 24).</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "A duração é obrigatória.")]
    [Range(1, 24, ErrorMessage = "A duração deve estar entre 1 e 24 meses.")]
    public int? DuracaoMeses { get; set; }

    /// <summary>Planos inativos não aceitam novas matrículas. Padrão: true.</summary>
    /// <example>true</example>
    public bool Ativo { get; set; } = true;
}

/// <summary>Plano retornado pela API.</summary>
public class PlanoResponse
{
    /// <example>4</example>
    public int Id { get; set; }
    /// <example>Anual</example>
    public string Nome { get; set; } = string.Empty;
    /// <example>12 meses de acesso com o melhor preço.</example>
    public string? Descricao { get; set; }
    /// <example>89.90</example>
    public decimal ValorMensal { get; set; }
    /// <example>12</example>
    public int DuracaoMeses { get; set; }
    /// <summary>Valor total do plano (ValorMensal × DuracaoMeses).</summary>
    /// <example>1078.80</example>
    public decimal ValorTotal { get; set; }
    /// <example>true</example>
    public bool Ativo { get; set; }
}

// =====================================================================
//  INSTRUTORES
// =====================================================================

/// <summary>Dados para cadastrar ou atualizar um instrutor.</summary>
public class InstrutorRequest
{
    /// <summary>Nome completo (3 a 100 caracteres).</summary>
    /// <example>Patrícia Gomes</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Modalidade em que o instrutor atua.</summary>
    /// <example>Pilates</example>
    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "A especialidade deve ter entre 2 e 60 caracteres.")]
    public string Especialidade { get; set; } = string.Empty;

    /// <summary>E-mail válido e único.</summary>
    /// <example>patricia.gomes@academia.exemplo.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;
}

/// <summary>Instrutor retornado pela API.</summary>
public class InstrutorResponse
{
    /// <example>1</example>
    public int Id { get; set; }
    /// <example>Marcos Pereira</example>
    public string Nome { get; set; } = string.Empty;
    /// <example>Spinning</example>
    public string Especialidade { get; set; } = string.Empty;
    /// <example>marcos.pereira@academia.exemplo.com</example>
    public string Email { get; set; } = string.Empty;
    /// <summary>Quantidade de aulas atribuídas ao instrutor.</summary>
    /// <example>2</example>
    public int QuantidadeAulas { get; set; }
}

// =====================================================================
//  AULAS
// =====================================================================

/// <summary>Dados para cadastrar ou atualizar uma aula coletiva.</summary>
public class AulaRequest
{
    /// <summary>Título da aula (3 a 80 caracteres).</summary>
    /// <example>Pilates Solo</example>
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 80 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Sala ou local da aula (opcional).</summary>
    /// <example>Sala Zen</example>
    [StringLength(40)]
    public string? Sala { get; set; }

    /// <summary>Data e hora de início (formato AAAA-MM-DDTHH:MM:SS). Deve estar no futuro.</summary>
    /// <example>2027-06-10T08:00:00</example>
    [Required(ErrorMessage = "A data/hora de início é obrigatória.")]
    public DateTime? DataHoraInicio { get; set; }

    /// <summary>Duração em minutos (15 a 180).</summary>
    /// <example>50</example>
    [Required(ErrorMessage = "A duração é obrigatória.")]
    [Range(15, 180, ErrorMessage = "A duração deve estar entre 15 e 180 minutos.")]
    public int? DuracaoMinutos { get; set; }

    /// <summary>Número máximo de alunos (1 a 100).</summary>
    /// <example>12</example>
    [Required(ErrorMessage = "A capacidade é obrigatória.")]
    [Range(1, 100, ErrorMessage = "A capacidade deve estar entre 1 e 100 alunos.")]
    public int? Capacidade { get; set; }

    /// <summary>Id de um instrutor existente (consulte GET /api/instrutores).</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "O instrutor é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um id de instrutor válido.")]
    public int? InstrutorId { get; set; }
}

/// <summary>Aula retornada pela API, com ocupação de vagas.</summary>
public class AulaResponse
{
    /// <example>1</example>
    public int Id { get; set; }
    /// <example>Spinning Intenso</example>
    public string Titulo { get; set; } = string.Empty;
    /// <example>Sala Bike</example>
    public string? Sala { get; set; }
    /// <example>2027-06-07T07:00:00</example>
    public DateTime DataHoraInicio { get; set; }
    /// <example>45</example>
    public int DuracaoMinutos { get; set; }
    /// <example>20</example>
    public int Capacidade { get; set; }
    /// <summary>Quantidade de reservas confirmadas.</summary>
    /// <example>1</example>
    public int VagasOcupadas { get; set; }
    /// <summary>Capacidade − reservas confirmadas.</summary>
    /// <example>19</example>
    public int VagasDisponiveis { get; set; }
    /// <example>1</example>
    public int InstrutorId { get; set; }
    /// <example>Marcos Pereira</example>
    public string InstrutorNome { get; set; } = string.Empty;
}

// =====================================================================
//  MATRÍCULAS (regra de negócio)
// =====================================================================

/// <summary>Dados para matricular um aluno em um plano.</summary>
public class MatriculaRequest
{
    /// <summary>Id do aluno (deve existir, estar ativo e não ter matrícula vigente).</summary>
    /// <example>5</example>
    [Required(ErrorMessage = "O aluno é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um id de aluno válido.")]
    public int? AlunoId { get; set; }

    /// <summary>Id do plano (deve existir e estar ativo).</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "O plano é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um id de plano válido.")]
    public int? PlanoId { get; set; }

    /// <summary>Data de início (AAAA-MM-DD). Opcional: se omitida, usa a data de hoje. Não pode estar no passado.</summary>
    /// <example>null</example>
    public DateOnly? DataInicio { get; set; }
}

/// <summary>Matrícula retornada pela API.</summary>
public class MatriculaResponse
{
    /// <example>1</example>
    public int Id { get; set; }
    /// <example>1</example>
    public int AlunoId { get; set; }
    /// <example>Ana Souza</example>
    public string AlunoNome { get; set; } = string.Empty;
    /// <example>4</example>
    public int PlanoId { get; set; }
    /// <example>Anual</example>
    public string PlanoNome { get; set; } = string.Empty;
    /// <example>2026-11-01</example>
    public DateOnly DataInicio { get; set; }
    /// <summary>Calculada: DataInicio + duração do plano − 1 dia.</summary>
    /// <example>2027-10-31</example>
    public DateOnly DataFim { get; set; }
    /// <summary>Calculado: ValorMensal × DuracaoMeses do plano.</summary>
    /// <example>1078.80</example>
    public decimal ValorTotal { get; set; }
    /// <summary>Ativa, Cancelada ou Expirada (Expirada = DataFim já passou).</summary>
    /// <example>Ativa</example>
    public StatusMatricula Status { get; set; }
    /// <example>null</example>
    public DateTime? DataCancelamento { get; set; }
}

// =====================================================================
//  RESERVAS (regra de negócio)
// =====================================================================

/// <summary>Dados para reservar uma vaga em uma aula.</summary>
public class ReservaRequest
{
    /// <summary>Id da aula (deve existir, ainda não ter começado e ter vaga).</summary>
    /// <example>1</example>
    [Required(ErrorMessage = "A aula é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um id de aula válido.")]
    public int? AulaId { get; set; }

    /// <summary>Id do aluno (deve estar ativo e com matrícula vigente).</summary>
    /// <example>3</example>
    [Required(ErrorMessage = "O aluno é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um id de aluno válido.")]
    public int? AlunoId { get; set; }
}

/// <summary>Reserva retornada pela API.</summary>
public class ReservaResponse
{
    /// <example>1</example>
    public int Id { get; set; }
    /// <example>1</example>
    public int AulaId { get; set; }
    /// <example>Spinning Intenso</example>
    public string AulaTitulo { get; set; } = string.Empty;
    /// <example>2027-06-07T07:00:00</example>
    public DateTime AulaDataHoraInicio { get; set; }
    /// <example>1</example>
    public int AlunoId { get; set; }
    /// <example>Ana Souza</example>
    public string AlunoNome { get; set; } = string.Empty;
    /// <example>2026-11-20T10:00:00</example>
    public DateTime DataReserva { get; set; }
    /// <summary>Confirmada ou Cancelada.</summary>
    /// <example>Confirmada</example>
    public StatusReserva Status { get; set; }
    /// <example>null</example>
    public DateTime? DataCancelamento { get; set; }
}
