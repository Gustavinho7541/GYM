using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AcademiaApi.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicialComDadosExemplo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alunos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alunos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instrutores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Especialidade = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrutores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Planos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ValorMensal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    DuracaoMeses = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Aulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Sala = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    DataHoraInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacidade = table.Column<int>(type: "INTEGER", nullable: false),
                    InstrutorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aulas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aulas_Instrutores_InstrutorId",
                        column: x => x.InstrutorId,
                        principalTable: "Instrutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Matriculas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matriculas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matriculas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matriculas_Planos_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "Planos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AulaId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataReserva = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Aulas_AulaId",
                        column: x => x.AulaId,
                        principalTable: "Aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Alunos",
                columns: new[] { "Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone" },
                values: new object[,]
                {
                    { 1, true, "11122233301", new DateTime(2026, 1, 10, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1995, 3, 12), "ana.souza@exemplo.com", "Ana Souza", "(11) 98888-0001" },
                    { 2, true, "11122233302", new DateTime(2026, 2, 3, 18, 30, 0, 0, DateTimeKind.Unspecified), new DateOnly(1990, 7, 25), "bruno.lima@exemplo.com", "Bruno Lima", "(11) 98888-0002" },
                    { 3, true, "11122233303", new DateTime(2026, 5, 20, 7, 15, 0, 0, DateTimeKind.Unspecified), new DateOnly(2000, 11, 2), "carla.mendes@exemplo.com", "Carla Mendes", "(11) 98888-0003" },
                    { 4, true, "11122233304", new DateTime(2026, 6, 1, 20, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1988, 1, 30), "diego.rocha@exemplo.com", "Diego Rocha", "(11) 98888-0004" },
                    { 5, true, "11122233305", new DateTime(2026, 11, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1999, 9, 9), "elisa.ferreira@exemplo.com", "Elisa Ferreira", null },
                    { 6, false, "11122233306", new DateTime(2025, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1985, 5, 5), "fabio.nunes@exemplo.com", "Fábio Nunes", "(11) 98888-0006" }
                });

            migrationBuilder.InsertData(
                table: "Instrutores",
                columns: new[] { "Id", "Email", "Especialidade", "Nome" },
                values: new object[,]
                {
                    { 1, "marcos.pereira@academia.exemplo.com", "Spinning", "Marcos Pereira" },
                    { 2, "juliana.alves@academia.exemplo.com", "Yoga", "Juliana Alves" },
                    { 3, "rafael.costa@academia.exemplo.com", "Funcional", "Rafael Costa" }
                });

            migrationBuilder.InsertData(
                table: "Planos",
                columns: new[] { "Id", "Ativo", "Descricao", "DuracaoMeses", "Nome", "ValorMensal" },
                values: new object[,]
                {
                    { 1, true, "Acesso livre à musculação e aulas coletivas por 1 mês.", 1, "Mensal", 129.90m },
                    { 2, true, "3 meses de acesso com desconto.", 3, "Trimestral", 109.90m },
                    { 3, true, "6 meses de acesso com desconto.", 6, "Semestral", 99.90m },
                    { 4, true, "12 meses de acesso com o melhor preço.", 12, "Anual", 89.90m },
                    { 5, false, "Plano promocional encerrado (inativo).", 3, "Promo Verão 2025", 79.90m }
                });

            migrationBuilder.InsertData(
                table: "Aulas",
                columns: new[] { "Id", "Capacidade", "DataHoraInicio", "DuracaoMinutos", "InstrutorId", "Sala", "Titulo" },
                values: new object[,]
                {
                    { 1, 20, new DateTime(2027, 6, 7, 7, 0, 0, 0, DateTimeKind.Unspecified), 45, 1, "Sala Bike", "Spinning Intenso" },
                    { 2, 15, new DateTime(2027, 6, 7, 19, 0, 0, 0, DateTimeKind.Unspecified), 60, 2, "Sala Zen", "Yoga Flow" },
                    { 3, 2, new DateTime(2027, 6, 8, 12, 0, 0, 0, DateTimeKind.Unspecified), 30, 3, "Área Externa", "Funcional Express" },
                    { 4, 20, new DateTime(2027, 6, 9, 18, 0, 0, 0, DateTimeKind.Unspecified), 45, 1, "Sala Bike", "Spinning Iniciante" }
                });

            migrationBuilder.InsertData(
                table: "Matriculas",
                columns: new[] { "Id", "AlunoId", "DataCancelamento", "DataFim", "DataInicio", "PlanoId", "Status", "ValorTotal" },
                values: new object[,]
                {
                    { 1, 1, null, new DateOnly(2027, 10, 31), new DateOnly(2026, 11, 1), 4, "Ativa", 1078.80m },
                    { 2, 2, null, new DateOnly(2027, 3, 31), new DateOnly(2026, 10, 1), 3, "Ativa", 599.40m },
                    { 3, 3, null, new DateOnly(2027, 9, 14), new DateOnly(2026, 9, 15), 4, "Ativa", 1078.80m },
                    { 4, 4, new DateTime(2026, 8, 15, 14, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2026, 8, 31), new DateOnly(2026, 8, 1), 1, "Cancelada", 129.90m }
                });

            migrationBuilder.InsertData(
                table: "Reservas",
                columns: new[] { "Id", "AlunoId", "AulaId", "DataCancelamento", "DataReserva", "Status" },
                values: new object[,]
                {
                    { 1, 1, 1, null, new DateTime(2026, 11, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), "Confirmada" },
                    { 2, 2, 2, null, new DateTime(2026, 11, 20, 11, 0, 0, 0, DateTimeKind.Unspecified), "Confirmada" },
                    { 3, 1, 3, null, new DateTime(2026, 11, 21, 8, 0, 0, 0, DateTimeKind.Unspecified), "Confirmada" },
                    { 4, 2, 3, null, new DateTime(2026, 11, 21, 9, 0, 0, 0, DateTimeKind.Unspecified), "Confirmada" },
                    { 5, 3, 2, new DateTime(2026, 11, 22, 15, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 22, 9, 0, 0, 0, DateTimeKind.Unspecified), "Cancelada" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_Cpf",
                table: "Alunos",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alunos_Email",
                table: "Alunos",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_InstrutorId",
                table: "Aulas",
                column: "InstrutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrutores_Email",
                table: "Instrutores",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_AlunoId",
                table: "Matriculas",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_PlanoId",
                table: "Matriculas",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_AlunoId",
                table: "Reservas",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_AulaId",
                table: "Reservas",
                column: "AulaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matriculas");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Planos");

            migrationBuilder.DropTable(
                name: "Alunos");

            migrationBuilder.DropTable(
                name: "Aulas");

            migrationBuilder.DropTable(
                name: "Instrutores");
        }
    }
}
