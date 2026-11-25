CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Alunos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Alunos" PRIMARY KEY AUTOINCREMENT,
    "Nome" TEXT NOT NULL,
    "Cpf" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Telefone" TEXT NULL,
    "DataNascimento" TEXT NOT NULL,
    "DataCadastro" TEXT NOT NULL,
    "Ativo" INTEGER NOT NULL
);

CREATE TABLE "Instrutores" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Instrutores" PRIMARY KEY AUTOINCREMENT,
    "Nome" TEXT NOT NULL,
    "Especialidade" TEXT NOT NULL,
    "Email" TEXT NOT NULL
);

CREATE TABLE "Planos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Planos" PRIMARY KEY AUTOINCREMENT,
    "Nome" TEXT NOT NULL,
    "Descricao" TEXT NULL,
    "ValorMensal" TEXT NOT NULL,
    "DuracaoMeses" INTEGER NOT NULL,
    "Ativo" INTEGER NOT NULL
);

CREATE TABLE "Aulas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Aulas" PRIMARY KEY AUTOINCREMENT,
    "Titulo" TEXT NOT NULL,
    "Sala" TEXT NULL,
    "DataHoraInicio" TEXT NOT NULL,
    "DuracaoMinutos" INTEGER NOT NULL,
    "Capacidade" INTEGER NOT NULL,
    "InstrutorId" INTEGER NOT NULL,
    CONSTRAINT "FK_Aulas_Instrutores_InstrutorId" FOREIGN KEY ("InstrutorId") REFERENCES "Instrutores" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Matriculas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Matriculas" PRIMARY KEY AUTOINCREMENT,
    "AlunoId" INTEGER NOT NULL,
    "PlanoId" INTEGER NOT NULL,
    "DataInicio" TEXT NOT NULL,
    "DataFim" TEXT NOT NULL,
    "ValorTotal" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "DataCancelamento" TEXT NULL,
    CONSTRAINT "FK_Matriculas_Alunos_AlunoId" FOREIGN KEY ("AlunoId") REFERENCES "Alunos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Matriculas_Planos_PlanoId" FOREIGN KEY ("PlanoId") REFERENCES "Planos" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Reservas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Reservas" PRIMARY KEY AUTOINCREMENT,
    "AulaId" INTEGER NOT NULL,
    "AlunoId" INTEGER NOT NULL,
    "DataReserva" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "DataCancelamento" TEXT NULL,
    CONSTRAINT "FK_Reservas_Alunos_AlunoId" FOREIGN KEY ("AlunoId") REFERENCES "Alunos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reservas_Aulas_AulaId" FOREIGN KEY ("AulaId") REFERENCES "Aulas" ("Id") ON DELETE CASCADE
);

INSERT INTO "Alunos" ("Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone")
VALUES (1, 1, '11122233301', '2026-01-10 09:00:00', '1995-03-12', 'ana.souza@exemplo.com', 'Ana Souza', '(11) 98888-0001');
SELECT changes();

INSERT INTO "Alunos" ("Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone")
VALUES (2, 1, '11122233302', '2026-02-03 18:30:00', '1990-07-25', 'bruno.lima@exemplo.com', 'Bruno Lima', '(11) 98888-0002');
SELECT changes();

INSERT INTO "Alunos" ("Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone")
VALUES (3, 1, '11122233303', '2026-05-20 07:15:00', '2000-11-02', 'carla.mendes@exemplo.com', 'Carla Mendes', '(11) 98888-0003');
SELECT changes();

INSERT INTO "Alunos" ("Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone")
VALUES (4, 1, '11122233304', '2026-06-01 20:00:00', '1988-01-30', 'diego.rocha@exemplo.com', 'Diego Rocha', '(11) 98888-0004');
SELECT changes();

INSERT INTO "Alunos" ("Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone")
VALUES (5, 1, '11122233305', '2026-11-01 10:00:00', '1999-09-09', 'elisa.ferreira@exemplo.com', 'Elisa Ferreira', NULL);
SELECT changes();

INSERT INTO "Alunos" ("Id", "Ativo", "Cpf", "DataCadastro", "DataNascimento", "Email", "Nome", "Telefone")
VALUES (6, 0, '11122233306', '2025-03-01 08:00:00', '1985-05-05', 'fabio.nunes@exemplo.com', 'Fábio Nunes', '(11) 98888-0006');
SELECT changes();


INSERT INTO "Instrutores" ("Id", "Email", "Especialidade", "Nome")
VALUES (1, 'marcos.pereira@academia.exemplo.com', 'Spinning', 'Marcos Pereira');
SELECT changes();

INSERT INTO "Instrutores" ("Id", "Email", "Especialidade", "Nome")
VALUES (2, 'juliana.alves@academia.exemplo.com', 'Yoga', 'Juliana Alves');
SELECT changes();

INSERT INTO "Instrutores" ("Id", "Email", "Especialidade", "Nome")
VALUES (3, 'rafael.costa@academia.exemplo.com', 'Funcional', 'Rafael Costa');
SELECT changes();


INSERT INTO "Planos" ("Id", "Ativo", "Descricao", "DuracaoMeses", "Nome", "ValorMensal")
VALUES (1, 1, 'Acesso livre à musculação e aulas coletivas por 1 mês.', 1, 'Mensal', '129.9');
SELECT changes();

INSERT INTO "Planos" ("Id", "Ativo", "Descricao", "DuracaoMeses", "Nome", "ValorMensal")
VALUES (2, 1, '3 meses de acesso com desconto.', 3, 'Trimestral', '109.9');
SELECT changes();

INSERT INTO "Planos" ("Id", "Ativo", "Descricao", "DuracaoMeses", "Nome", "ValorMensal")
VALUES (3, 1, '6 meses de acesso com desconto.', 6, 'Semestral', '99.9');
SELECT changes();

INSERT INTO "Planos" ("Id", "Ativo", "Descricao", "DuracaoMeses", "Nome", "ValorMensal")
VALUES (4, 1, '12 meses de acesso com o melhor preço.', 12, 'Anual', '89.9');
SELECT changes();

INSERT INTO "Planos" ("Id", "Ativo", "Descricao", "DuracaoMeses", "Nome", "ValorMensal")
VALUES (5, 0, 'Plano promocional encerrado (inativo).', 3, 'Promo Verão 2025', '79.9');
SELECT changes();


INSERT INTO "Aulas" ("Id", "Capacidade", "DataHoraInicio", "DuracaoMinutos", "InstrutorId", "Sala", "Titulo")
VALUES (1, 20, '2027-06-07 07:00:00', 45, 1, 'Sala Bike', 'Spinning Intenso');
SELECT changes();

INSERT INTO "Aulas" ("Id", "Capacidade", "DataHoraInicio", "DuracaoMinutos", "InstrutorId", "Sala", "Titulo")
VALUES (2, 15, '2027-06-07 19:00:00', 60, 2, 'Sala Zen', 'Yoga Flow');
SELECT changes();

INSERT INTO "Aulas" ("Id", "Capacidade", "DataHoraInicio", "DuracaoMinutos", "InstrutorId", "Sala", "Titulo")
VALUES (3, 2, '2027-06-08 12:00:00', 30, 3, 'Área Externa', 'Funcional Express');
SELECT changes();

INSERT INTO "Aulas" ("Id", "Capacidade", "DataHoraInicio", "DuracaoMinutos", "InstrutorId", "Sala", "Titulo")
VALUES (4, 20, '2027-06-09 18:00:00', 45, 1, 'Sala Bike', 'Spinning Iniciante');
SELECT changes();


INSERT INTO "Matriculas" ("Id", "AlunoId", "DataCancelamento", "DataFim", "DataInicio", "PlanoId", "Status", "ValorTotal")
VALUES (1, 1, NULL, '2027-10-31', '2026-11-01', 4, 'Ativa', '1078.8');
SELECT changes();

INSERT INTO "Matriculas" ("Id", "AlunoId", "DataCancelamento", "DataFim", "DataInicio", "PlanoId", "Status", "ValorTotal")
VALUES (2, 2, NULL, '2027-03-31', '2026-10-01', 3, 'Ativa', '599.4');
SELECT changes();

INSERT INTO "Matriculas" ("Id", "AlunoId", "DataCancelamento", "DataFim", "DataInicio", "PlanoId", "Status", "ValorTotal")
VALUES (3, 3, NULL, '2027-09-14', '2026-09-15', 4, 'Ativa', '1078.8');
SELECT changes();

INSERT INTO "Matriculas" ("Id", "AlunoId", "DataCancelamento", "DataFim", "DataInicio", "PlanoId", "Status", "ValorTotal")
VALUES (4, 4, '2026-08-15 14:00:00', '2026-08-31', '2026-08-01', 1, 'Cancelada', '129.9');
SELECT changes();


INSERT INTO "Reservas" ("Id", "AlunoId", "AulaId", "DataCancelamento", "DataReserva", "Status")
VALUES (1, 1, 1, NULL, '2026-11-20 10:00:00', 'Confirmada');
SELECT changes();

INSERT INTO "Reservas" ("Id", "AlunoId", "AulaId", "DataCancelamento", "DataReserva", "Status")
VALUES (2, 2, 2, NULL, '2026-11-20 11:00:00', 'Confirmada');
SELECT changes();

INSERT INTO "Reservas" ("Id", "AlunoId", "AulaId", "DataCancelamento", "DataReserva", "Status")
VALUES (3, 1, 3, NULL, '2026-11-21 08:00:00', 'Confirmada');
SELECT changes();

INSERT INTO "Reservas" ("Id", "AlunoId", "AulaId", "DataCancelamento", "DataReserva", "Status")
VALUES (4, 2, 3, NULL, '2026-11-21 09:00:00', 'Confirmada');
SELECT changes();

INSERT INTO "Reservas" ("Id", "AlunoId", "AulaId", "DataCancelamento", "DataReserva", "Status")
VALUES (5, 3, 2, '2026-11-22 15:00:00', '2026-11-22 09:00:00', 'Cancelada');
SELECT changes();


CREATE UNIQUE INDEX "IX_Alunos_Cpf" ON "Alunos" ("Cpf");

CREATE UNIQUE INDEX "IX_Alunos_Email" ON "Alunos" ("Email");

CREATE INDEX "IX_Aulas_InstrutorId" ON "Aulas" ("InstrutorId");

CREATE UNIQUE INDEX "IX_Instrutores_Email" ON "Instrutores" ("Email");

CREATE INDEX "IX_Matriculas_AlunoId" ON "Matriculas" ("AlunoId");

CREATE INDEX "IX_Matriculas_PlanoId" ON "Matriculas" ("PlanoId");

CREATE INDEX "IX_Reservas_AlunoId" ON "Reservas" ("AlunoId");

CREATE INDEX "IX_Reservas_AulaId" ON "Reservas" ("AulaId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261123225925_CriacaoInicialComDadosExemplo', '10.0.15');

COMMIT;

