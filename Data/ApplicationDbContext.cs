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
        public DbSet<DocumentDownloadLog> DocumentDownloadLogs { get; set; }
        public DbSet<Ramal> Ramais { get; set; }
        public DbSet<Reuniao> Reunioes { get; set; }
        public DbSet<ReuniaoParticipante> ReuniaoParticipantes { get; set; }
        
        // Workflow entities
        public DbSet<DocumentWorkflow> DocumentWorkflows { get; set; }
        public DbSet<DocumentHistory> DocumentHistories { get; set; }
        public DbSet<DocumentApprover> DocumentApprovers { get; set; }
        public DbSet<DocumentReviewer> DocumentReviewers { get; set; }

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

            builder.Entity<Document>()
                .HasOne(d => d.LastModifiedBy)
                .WithMany()
                .HasForeignKey(d => d.LastModifiedById)
                .OnDelete(DeleteBehavior.SetNull);

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

            // Configuração das relações para DocumentWorkflow
            builder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.Document)
                .WithMany(d => d.Workflows)
                .HasForeignKey(dw => dw.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.User)
                .WithMany()
                .HasForeignKey(dw => dw.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.AssignedToUser)
                .WithMany()
                .HasForeignKey(dw => dw.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuração das relações para DocumentHistory
            builder.Entity<DocumentHistory>()
                .HasOne(dh => dh.Document)
                .WithMany(d => d.History)
                .HasForeignKey(dh => dh.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentHistory>()
                .HasOne(dh => dh.User)
                .WithMany()
                .HasForeignKey(dh => dh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuração das relações para DocumentApprover
            builder.Entity<DocumentApprover>()
                .HasOne(da => da.Department)
                .WithMany()
                .HasForeignKey(da => da.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentApprover>()
                .HasOne(da => da.User)
                .WithMany()
                .HasForeignKey(da => da.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração das relações para DocumentReviewer
            builder.Entity<DocumentReviewer>()
                .HasOne(dr => dr.Department)
                .WithMany()
                .HasForeignKey(dr => dr.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DocumentReviewer>()
                .HasOne(dr => dr.User)
                .WithMany()
                .HasForeignKey(dr => dr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.Entity<DocumentWorkflow>()
                .HasIndex(dw => new { dw.DocumentId, dw.CurrentStatus });

            builder.Entity<DocumentHistory>()
                .HasIndex(dh => new { dh.DocumentId, dh.CreatedAt });

            builder.Entity<Document>()
                .HasIndex(d => new { d.Status, d.DepartmentId });

            builder.Entity<DocumentApprover>()
                .HasIndex(da => new { da.DepartmentId, da.Order, da.IsActive });

            builder.Entity<DocumentReviewer>()
                .HasIndex(dr => new { dr.DepartmentId, dr.IsActive });

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
