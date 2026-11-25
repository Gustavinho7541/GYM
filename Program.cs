using System.Reflection;
using System.Text.Json.Serialization;
using AcademiaApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------- Banco de dados (SQLite, arquivo academia.db) ----------
builder.Services.AddDbContext<AcademiaContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Academia")));

// ---------- Controllers + JSON (enums como texto: "Ativa", "Confirmada"...) ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o =>
    {
        // Resposta 400 padronizada (ValidationProblemDetails) com título em português
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var problema = new ValidationProblemDetails(ctx.ModelState)
            {
                Title = "Um ou mais campos são inválidos.",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Corrija os campos listados em 'errors' e envie novamente."
            };
            return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
        };
    });

// ---------- Swagger / OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Academia API",
        Version = "v1",
        Description = """
            API REST para gestão de uma academia: **alunos, planos, matrículas, instrutores, aulas coletivas e reservas de vagas**.

            ### Como usar
            1. Abra um endpoint, clique em **Try it out**, preencha os parâmetros/corpo e clique em **Execute**.
            2. Os corpos de exemplo já vêm preenchidos com dados válidos.
            3. O banco já contém dados de exemplo (alunos 1–6, planos 1–5, instrutores 1–3, aulas 1–4).

            ### Fluxo sugerido
            `GET /api/alunos` → `POST /api/matriculas` (aluno 5) → `GET /api/aulas?comVagas=true` → `POST /api/reservas` → `POST /api/reservas/{id}/cancelar`

            ### Códigos HTTP
            | Código | Significado |
            |---|---|
            | 200 | Sucesso com corpo |
            | 201 | Recurso criado (cabeçalho `Location` aponta para ele) |
            | 204 | Excluído com sucesso (sem corpo) |
            | 400 | Dados inválidos ou regra violada — veja `errors` / `detail` |
            | 404 | Recurso não encontrado (id da rota ou id referenciado no corpo) |
            | 409 | Conflito: duplicidade, aula lotada, matrícula já vigente, vínculo impede exclusão |

            Todas as respostas de erro seguem o formato **ProblemDetails** (`title`, `status`, `detail`).
            Datas: `AAAA-MM-DD`; data/hora: `AAAA-MM-DDTHH:MM:SS` (horário local).
            """
    });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    c.IncludeXmlComments(xml, includeControllerXmlComments: true);
});

var app = builder.Build();

// ---------- Cria/atualiza o banco e insere os dados de exemplo na inicialização ----------
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AcademiaContext>().Database.Migrate();
}

// Swagger habilitado em qualquer ambiente (necessário para a demonstração)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academia API v1");
    c.RoutePrefix = string.Empty; // Swagger UI na raiz: http://localhost:5267/
    c.DocumentTitle = "Academia API — Swagger";
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
    c.DefaultModelsExpandDepth(1);
});

// Front-end (HTML/JS) em wwwroot/app  ->  http://localhost:5267/app/
app.UseStaticFiles();

app.MapControllers();
app.Run();
