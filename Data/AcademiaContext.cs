using AcademiaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademiaApi.Data;

public class AcademiaContext(DbContextOptions<AcademiaContext> options) : DbContext(options)
{
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Instrutor> Instrutores => Set<Instrutor>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---------- Aluno ----------
        mb.Entity<Aluno>(e =>
        {
            e.Property(a => a.Nome).HasMaxLength(100).IsRequired();
            e.Property(a => a.Cpf).HasMaxLength(11).IsRequired();
            e.Property(a => a.Email).HasMaxLength(120).IsRequired();
            e.Property(a => a.Telefone).HasMaxLength(20);
            e.HasIndex(a => a.Cpf).IsUnique();
            e.HasIndex(a => a.Email).IsUnique();
        });

        // ---------- Plano ----------
        mb.Entity<Plano>(e =>
        {
            e.Property(p => p.Nome).HasMaxLength(60).IsRequired();
            e.Property(p => p.Descricao).HasMaxLength(300);
            e.Property(p => p.ValorMensal).HasPrecision(10, 2);
        });

        // ---------- Matricula (Aluno 1:N Matricula N:1 Plano) ----------
        mb.Entity<Matricula>(e =>
        {
            e.Property(m => m.ValorTotal).HasPrecision(10, 2);
            e.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(m => m.Aluno).WithMany(a => a.Matriculas)
                .HasForeignKey(m => m.AlunoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Plano).WithMany(p => p.Matriculas)
                .HasForeignKey(m => m.PlanoId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Instrutor ----------
        mb.Entity<Instrutor>(e =>
        {
            e.Property(i => i.Nome).HasMaxLength(100).IsRequired();
            e.Property(i => i.Especialidade).HasMaxLength(60).IsRequired();
            e.Property(i => i.Email).HasMaxLength(120).IsRequired();
            e.HasIndex(i => i.Email).IsUnique();
        });

        // ---------- Aula (Instrutor 1:N Aula) ----------
        mb.Entity<Aula>(e =>
        {
            e.Property(a => a.Titulo).HasMaxLength(80).IsRequired();
            e.Property(a => a.Sala).HasMaxLength(40);
            e.HasOne(a => a.Instrutor).WithMany(i => i.Aulas)
                .HasForeignKey(a => a.InstrutorId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Reserva (Aluno N:N Aula) ----------
        mb.Entity<Reserva>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(r => r.Aula).WithMany(a => a.Reservas)
                .HasForeignKey(r => r.AulaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Aluno).WithMany(a => a.Reservas)
                .HasForeignKey(r => r.AlunoId).OnDelete(DeleteBehavior.Cascade);
        });

        SeedData.Aplicar(mb);
    }
}
