using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IntranetDocumentos.Models;

namespace IntranetDocumentos.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }        public DbSet<Department> Departments { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentDownload> DocumentDownloads { get; set; }
        public DbSet<DocumentDownloadLog> DocumentDownloadLogs { get; set; }
        public DbSet<Ramal> Ramais { get; set; }
        public DbSet<Reuniao> Reunioes { get; set; }
        public DbSet<ReuniaoParticipante> ReuniaoParticipantes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuração das relações
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Document>()
                .HasOne(d => d.Uploader)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(d => d.UploaderId)
                .OnDelete(DeleteBehavior.Restrict);            builder.Entity<Document>()
                .HasOne(d => d.Department)
                .WithMany(dep => dep.Documents)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuração das relações para DocumentDownload
            builder.Entity<DocumentDownload>()
                .HasOne(dd => dd.Document)
                .WithMany()
                .HasForeignKey(dd => dd.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentDownload>()
                .HasOne(dd => dd.User)
                .WithMany()
                .HasForeignKey(dd => dd.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração das relações para DocumentDownloadLog
            builder.Entity<DocumentDownloadLog>()
                .HasOne(ddl => ddl.Document)
                .WithMany()
                .HasForeignKey(ddl => ddl.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentDownloadLog>()
                .HasOne(ddl => ddl.User)
                .WithMany()
                .HasForeignKey(ddl => ddl.UserId)
                .OnDelete(DeleteBehavior.Cascade);            // Configuração das relações para Ramal
            builder.Entity<Ramal>()
                .HasOne(r => r.Department)                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuração das relações para ReuniaoParticipante
            builder.Entity<ReuniaoParticipante>()
                .HasOne(rp => rp.Reuniao)
                .WithMany(r => r.Participantes)
                .HasForeignKey(rp => rp.ReuniaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data para departamentos
            builder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Pessoal" },
                new Department { Id = 2, Name = "Fiscal" },
                new Department { Id = 3, Name = "Contábil" },
                new Department { Id = 4, Name = "Cadastro" },
                new Department { Id = 5, Name = "Apoio" },
                new Department { Id = 6, Name = "TI" }
            );
        }
    }
}
