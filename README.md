# Academia API

API REST em **ASP.NET Core 10 + C#** para gestão de uma academia: alunos, planos, matrículas, instrutores, aulas coletivas e reservas de vagas.
Persistência em **SQLite** com **Entity Framework Core** e documentação interativa em **Swagger UI (OpenAPI)**.

---

## 1. Softwares necessários

| Software | Versão | Obrigatório? | Observação |
|---|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | **10.0** ou superior | Sim | Verifique com `dotnet --list-sdks` |
| Navegador (Chrome, Edge, Firefox) | qualquer recente | Sim | Para abrir o Swagger UI |
| Git | qualquer | Opcional | Para clonar o repositório |
| Visual Studio 2022/2026 ou VS Code + C# Dev Kit | — | Opcional | Para editar/depurar |
| DB Browser for SQLite | — | Opcional | Para visualizar o arquivo `academia.db` |

**Não é preciso instalar servidor de banco de dados**: o SQLite é um arquivo local (`academia.db`) criado automaticamente.
Os pacotes NuGet (EF Core 10, Swashbuckle) são baixados automaticamente no primeiro `dotnet run` (requer internet nessa primeira vez).

---

## 2. Como executar

```bash
cd AcademiaApi
dotnet run
```

Abra **http://localhost:5267** — o Swagger UI é a página inicial.

Na inicialização a API aplica as migrations automaticamente (`Database.Migrate()`): cria o banco `academia.db` e insere os dados de exemplo.

### Resetar os dados de exemplo
Pare a API (Ctrl+C), apague o banco e rode de novo:

```bash
# Windows (PowerShell)
Remove-Item academia.db*
# Linux/macOS
rm academia.db*

dotnet run
```

### Migrations e script SQL (opcional)
A ferramenta `dotnet-ef` está declarada no manifesto local (`dotnet-tools.json`):

```bash
dotnet tool restore                      # instala o dotnet-ef local
dotnet ef database update                # cria/atualiza o banco pelas migrations
dotnet ef migrations script -o Scripts/criar_banco_e_dados_exemplo.sql
```

O script já gerado está em [`Scripts/criar_banco_e_dados_exemplo.sql`](Scripts/criar_banco_e_dados_exemplo.sql) (cria as tabelas e insere os dados de exemplo).

---

## 3. Estrutura do projeto

```
AcademiaApi/
├── Controllers/        # Endpoints (Alunos, Planos, Instrutores, Aulas, Matriculas, Reservas)
├── Data/
│   ├── AcademiaContext.cs   # DbContext + relacionamentos
│   └── SeedData.cs          # Dados de exemplo (HasData)
├── Dtos/               # Modelos de entrada/saída com validações e exemplos do Swagger
├── Migrations/         # Migrations do EF Core
├── Models/             # Entidades do banco
├── Scripts/            # Script SQL equivalente às migrations
└── Program.cs          # Configuração: EF Core, JSON, Swagger
```

---

## 4. Modelo de dados

```
Aluno 1 ──── N Matricula N ──── 1 Plano
  │
  1
  │
  N
Reserva N ──── 1 Aula N ──── 1 Instrutor
```

| Entidade | Campos principais | Relacionamentos |
|---|---|---|
| **Aluno** | Nome, CPF (único), E-mail (único), Telefone, DataNascimento, Ativo | 1:N Matrículas, 1:N Reservas (exclusão em cascata) |
| **Plano** | Nome, Descrição, ValorMensal, DuracaoMeses, Ativo | 1:N Matrículas (exclusão bloqueada se houver matrículas) |
| **Matricula** | DataInicio, DataFim, ValorTotal, Status (Ativa/Cancelada/Expirada) | N:1 Aluno, N:1 Plano |
| **Instrutor** | Nome, Especialidade, E-mail (único) | 1:N Aulas (exclusão bloqueada se houver aulas) |
| **Aula** | Título, Sala, DataHoraInicio, DuracaoMinutos, Capacidade | N:1 Instrutor, 1:N Reservas (cascata) |
| **Reserva** | DataReserva, Status (Confirmada/Cancelada) | N:1 Aluno, N:1 Aula — tabela associativa Aluno N:N Aula |

---

## 5. Endpoints (31 operações)

### CRUD
| Recurso | GET lista | GET por id | POST | PUT | DELETE | Extras |
|---|---|---|---|---|---|---|
| `/api/alunos` | ✔ (`?nome=&ativo=`) | ✔ | ✔ | ✔ | ✔ | `GET /{id}/matriculas`, `GET /{id}/reservas` |
| `/api/planos` | ✔ (`?somenteAtivos=`) | ✔ | ✔ | ✔ | ✔ | |
| `/api/instrutores` | ✔ (`?especialidade=`) | ✔ | ✔ | ✔ | ✔ | |
| `/api/aulas` | ✔ (`?data=&instrutorId=&comVagas=`) | ✔ | ✔ | ✔ | ✔ | `GET /{id}/reservas` |

### Regras de negócio
| Método | Rota | O que faz |
|---|---|---|
| POST | `/api/matriculas` | **Matricula um aluno em um plano**: exige aluno e plano ativos, impede matrícula duplicada vigente, calcula `dataFim` e `valorTotal`. |
| POST | `/api/matriculas/{id}/cancelar` | **Cancela a matrícula** e cancela automaticamente as reservas futuras do aluno. |
| POST | `/api/reservas` | **Reserva vaga em aula**: exige matrícula vigente, aula futura, sem duplicidade e com vaga disponível. |
| POST | `/api/reservas/{id}/cancelar` | **Cancela a reserva** e libera a vaga. |
| GET | `/api/matriculas`, `/api/matriculas/{id}`, `/api/reservas`, `/api/reservas/{id}` | Consultas com filtros (`alunoId`, `aulaId`, `status`). |

### Códigos HTTP
| Código | Quando |
|---|---|
| 200 OK | Consulta, atualização ou cancelamento bem-sucedido |
| 201 Created | Recurso criado (cabeçalho `Location` com a URL) |
| 204 No Content | Exclusão bem-sucedida |
| 400 Bad Request | Campo obrigatório ausente, formato inválido, valor fora da faixa, data no passado, aluno sem matrícula |
| 404 Not Found | Id inexistente (na rota ou referenciado no corpo) |
| 409 Conflict | CPF/e-mail duplicado, aula lotada, reserva/matrícula duplicada, exclusão bloqueada por vínculo |

Erros seguem o padrão **ProblemDetails** (`title`, `status`, `detail`; erros de validação trazem também `errors` por campo).

---

## 6. Dados de exemplo

| Tipo | Registros |
|---|---|
| Planos | 1 Mensal · 2 Trimestral · 3 Semestral · 4 Anual · 5 Promo Verão (**inativo**) |
| Alunos | 1 Ana, 2 Bruno, 3 Carla (**com matrícula ativa**) · 4 Diego (matrícula **cancelada**) · 5 Elisa (**sem matrícula**) · 6 Fábio (**inativo**) |
| Instrutores | 1 Marcos (Spinning) · 2 Juliana (Yoga) · 3 Rafael (Funcional) |
| Aulas (jun/2027) | 1 Spinning Intenso · 2 Yoga Flow · 3 Funcional Express (**capacidade 2, lotada**) · 4 Spinning Iniciante |
| Reservas | 4 confirmadas + 1 cancelada |

---

## 7. Roteiro de demonstração no Swagger

1. **GET `/api/alunos`** → 200, lista dos alunos.
2. **GET `/api/alunos/1`** → 200. Depois **GET `/api/alunos/999`** → **404**.
3. **POST `/api/alunos`** com o exemplo já preenchido → **201** (veja o `Location`).
4. **POST `/api/alunos`** com dados inválidos → **400** com a lista de erros:
   ```json
   { "nome": "A", "cpf": "123", "email": "invalido", "dataNascimento": null }
   ```
5. **PUT `/api/alunos/{id}`** alterando o telefone → 200.
6. **DELETE `/api/alunos/6`** → **204**.
7. **Regra 1 — POST `/api/matriculas`** `{ "alunoId": 5, "planoId": 2 }` → **201** com `dataFim` e `valorTotal` calculados.
   Repita o mesmo POST → **409** (matrícula já vigente).
8. **Regra 2 — POST `/api/reservas`**
   - `{ "aulaId": 1, "alunoId": 3 }` → **201** (GET `/api/aulas/1` mostra `vagasDisponiveis` diminuindo);
   - `{ "aulaId": 3, "alunoId": 3 }` → **409** (aula lotada);
   - `{ "aulaId": 1, "alunoId": 4 }` → **400** (sem matrícula vigente).
9. **POST `/api/reservas/{id}/cancelar`** → 200 e a vaga é liberada.
10. **DELETE `/api/planos/1`** → **409** (plano tem matrículas).

### Desafio entre grupos (inclusão, consulta, alteração)
Sugestão para o grupo avaliador: `POST /api/instrutores` → `GET /api/instrutores/{id}` → `PUT /api/instrutores/{id}`. Todos os corpos vêm com exemplos válidos no Swagger.

---

## 8. Acesso por outro computador (opcional)

Para que outro grupo acesse a API na mesma rede:

```bash
dotnet run --urls http://0.0.0.0:5267
```

O outro grupo abre `http://<IP-da-sua-máquina>:5267`. Pode ser necessário liberar a porta 5267 no firewall do Windows.
