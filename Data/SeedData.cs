using AcademiaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Data;

/// <summary>
/// Dados de exemplo inseridos pela migration inicial (HasData).
/// Todos os dados são fictícios.
/// </summary>
public static class SeedData
{
    public static void Aplicar(ModelBuilder mb)
    {
        mb.Entity<Plano>().HasData(
            new Plano { Id = 1, Nome = "Mensal", Descricao = "Acesso livre à musculação e aulas coletivas por 1 mês.", ValorMensal = 129.90m, DuracaoMeses = 1, Ativo = true },
            new Plano { Id = 2, Nome = "Trimestral", Descricao = "3 meses de acesso com desconto.", ValorMensal = 109.90m, DuracaoMeses = 3, Ativo = true },
            new Plano { Id = 3, Nome = "Semestral", Descricao = "6 meses de acesso com desconto.", ValorMensal = 99.90m, DuracaoMeses = 6, Ativo = true },
            new Plano { Id = 4, Nome = "Anual", Descricao = "12 meses de acesso com o melhor preço.", ValorMensal = 89.90m, DuracaoMeses = 12, Ativo = true },
            new Plano { Id = 5, Nome = "Promo Verão 2025", Descricao = "Plano promocional encerrado (inativo).", ValorMensal = 79.90m, DuracaoMeses = 3, Ativo = false }
        );

        mb.Entity<Aluno>().HasData(
            new Aluno { Id = 1, Nome = "Ana Souza", Cpf = "11122233301", Email = "ana.souza@exemplo.com", Telefone = "(11) 98888-0001", DataNascimento = new DateOnly(1995, 3, 12), DataCadastro = new DateTime(2026, 1, 10, 9, 0, 0), Ativo = true },
            new Aluno { Id = 2, Nome = "Bruno Lima", Cpf = "11122233302", Email = "bruno.lima@exemplo.com", Telefone = "(11) 98888-0002", DataNascimento = new DateOnly(1990, 7, 25), DataCadastro = new DateTime(2026, 2, 3, 18, 30, 0), Ativo = true },
            new Aluno { Id = 3, Nome = "Carla Mendes", Cpf = "11122233303", Email = "carla.mendes@exemplo.com", Telefone = "(11) 98888-0003", DataNascimento = new DateOnly(2000, 11, 2), DataCadastro = new DateTime(2026, 5, 20, 7, 15, 0), Ativo = true },
            new Aluno { Id = 4, Nome = "Diego Rocha", Cpf = "11122233304", Email = "diego.rocha@exemplo.com", Telefone = "(11) 98888-0004", DataNascimento = new DateOnly(1988, 1, 30), DataCadastro = new DateTime(2026, 6, 1, 20, 0, 0), Ativo = true },
            new Aluno { Id = 5, Nome = "Elisa Ferreira", Cpf = "11122233305", Email = "elisa.ferreira@exemplo.com", Telefone = null, DataNascimento = new DateOnly(1999, 9, 9), DataCadastro = new DateTime(2026, 11, 1, 10, 0, 0), Ativo = true },
            new Aluno { Id = 6, Nome = "Fábio Nunes", Cpf = "11122233306", Email = "fabio.nunes@exemplo.com", Telefone = "(11) 98888-0006", DataNascimento = new DateOnly(1985, 5, 5), DataCadastro = new DateTime(2025, 3, 1, 8, 0, 0), Ativo = false }
        );

        // Ana, Bruno e Carla: matrícula ativa | Diego: matrícula cancelada | Elisa: sem matrícula (use para demonstrar a matrícula)
        mb.Entity<Matricula>().HasData(
            new Matricula { Id = 1, AlunoId = 1, PlanoId = 4, DataInicio = new DateOnly(2026, 11, 1), DataFim = new DateOnly(2027, 10, 31), ValorTotal = 1078.80m, Status = StatusMatricula.Ativa },
            new Matricula { Id = 2, AlunoId = 2, PlanoId = 3, DataInicio = new DateOnly(2026, 10, 1), DataFim = new DateOnly(2027, 3, 31), ValorTotal = 599.40m, Status = StatusMatricula.Ativa },
            new Matricula { Id = 3, AlunoId = 3, PlanoId = 4, DataInicio = new DateOnly(2026, 9, 15), DataFim = new DateOnly(2027, 9, 14), ValorTotal = 1078.80m, Status = StatusMatricula.Ativa },
            new Matricula { Id = 4, AlunoId = 4, PlanoId = 1, DataInicio = new DateOnly(2026, 8, 1), DataFim = new DateOnly(2026, 8, 31), ValorTotal = 129.90m, Status = StatusMatricula.Cancelada, DataCancelamento = new DateTime(2026, 8, 15, 14, 0, 0) }
        );

        mb.Entity<Instrutor>().HasData(
            new Instrutor { Id = 1, Nome = "Marcos Pereira", Especialidade = "Spinning", Email = "marcos.pereira@academia.exemplo.com" },
            new Instrutor { Id = 2, Nome = "Juliana Alves", Especialidade = "Yoga", Email = "juliana.alves@academia.exemplo.com" },
            new Instrutor { Id = 3, Nome = "Rafael Costa", Especialidade = "Funcional", Email = "rafael.costa@academia.exemplo.com" }
        );

        // Aulas futuras para permitir reservas durante a apresentação.
        // A aula 3 tem capacidade 2 e já está lotada (demonstração do 409).
        mb.Entity<Aula>().HasData(
            new Aula { Id = 1, Titulo = "Spinning Intenso", Sala = "Sala Bike", DataHoraInicio = new DateTime(2027, 6, 7, 7, 0, 0), DuracaoMinutos = 45, Capacidade = 20, InstrutorId = 1 },
            new Aula { Id = 2, Titulo = "Yoga Flow", Sala = "Sala Zen", DataHoraInicio = new DateTime(2027, 6, 7, 19, 0, 0), DuracaoMinutos = 60, Capacidade = 15, InstrutorId = 2 },
            new Aula { Id = 3, Titulo = "Funcional Express", Sala = "Área Externa", DataHoraInicio = new DateTime(2027, 6, 8, 12, 0, 0), DuracaoMinutos = 30, Capacidade = 2, InstrutorId = 3 },
            new Aula { Id = 4, Titulo = "Spinning Iniciante", Sala = "Sala Bike", DataHoraInicio = new DateTime(2027, 6, 9, 18, 0, 0), DuracaoMinutos = 45, Capacidade = 20, InstrutorId = 1 }
        );

        mb.Entity<Reserva>().HasData(
            new Reserva { Id = 1, AulaId = 1, AlunoId = 1, DataReserva = new DateTime(2026, 11, 20, 10, 0, 0), Status = StatusReserva.Confirmada },
            new Reserva { Id = 2, AulaId = 2, AlunoId = 2, DataReserva = new DateTime(2026, 11, 20, 11, 0, 0), Status = StatusReserva.Confirmada },
            new Reserva { Id = 3, AulaId = 3, AlunoId = 1, DataReserva = new DateTime(2026, 11, 21, 8, 0, 0), Status = StatusReserva.Confirmada },
            new Reserva { Id = 4, AulaId = 3, AlunoId = 2, DataReserva = new DateTime(2026, 11, 21, 9, 0, 0), Status = StatusReserva.Confirmada },
            new Reserva { Id = 5, AulaId = 2, AlunoId = 3, DataReserva = new DateTime(2026, 11, 22, 9, 0, 0), Status = StatusReserva.Cancelada, DataCancelamento = new DateTime(2026, 11, 22, 15, 0, 0) }
        );
    }
}
