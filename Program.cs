using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Services;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar para Windows Server / IIS
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            builder.WebHost.UseIISIntegration();
        }

        // Validação da connection string
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("A connection string 'DefaultConnection' não foi encontrada. Configure-a no appsettings.json.");
        }

        // Adiciona CORS restritivo via configuração
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0];
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        // Add services to the container.
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Sign in settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure cookie settings
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        // Add custom services
        builder.Services.AddScoped<IDocumentService, DocumentService>();
        builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
        builder.Services.AddScoped<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IReuniaoService, ReuniaoService>();
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        builder.Services.AddScoped<IWorkflowService, WorkflowService>();

        // Add notification services
        builder.Services.AddScoped<IntranetDocumentos.Services.Notifications.IEmailService, IntranetDocumentos.Services.Notifications.EmailService>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Notifications.INotificationService, IntranetDocumentos.Services.Notifications.NotificationService>();

        // Add background services
        builder.Services.AddHostedService<IntranetDocumentos.Services.Background.MeetingReminderBackgroundService>();

        // Configure request size limits for file uploads
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
            options.ValueLengthLimit = 100 * 1024 * 1024; // 100MB
            options.ValueCountLimit = 1024;
            options.KeyLengthLimit = 2048;
        });

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
        });

        // Registrar novos serviços de documento - ISP aplicado
        builder.Services.AddScoped<IntranetDocumentos.Services.Documents.IDocumentReader, IntranetDocumentos.Services.Documents.DocumentReader>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Documents.IDocumentWriter, IntranetDocumentos.Services.Documents.DocumentWriter>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Documents.IDocumentSecurity, IntranetDocumentos.Services.Documents.DocumentSecurity>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Documents.IDocumentDownloader, IntranetDocumentos.Services.Documents.DocumentDownloader>();

        // Registrar Factory e Processors - Strategy Pattern  
        builder.Services.AddScoped<IntranetDocumentos.Services.FileProcessing.FileProcessorFactory>();
        builder.Services.AddScoped<IntranetDocumentos.Services.FileProcessing.IFileProcessor, IntranetDocumentos.Services.FileProcessing.ImageFileProcessor>();
        builder.Services.AddScoped<IntranetDocumentos.Services.FileProcessing.IFileProcessor, IntranetDocumentos.Services.FileProcessing.DocumentFileProcessor>();
        builder.Services.AddScoped<IntranetDocumentos.Services.FileProcessing.IFileProcessor, IntranetDocumentos.Services.FileProcessing.ArchiveFileProcessor>();
        // Processador genérico deve ser registrado por último para ter menor prioridade
        builder.Services.AddScoped<IntranetDocumentos.Services.FileProcessing.IFileProcessor, IntranetDocumentos.Services.FileProcessing.GenericFileProcessor>();

        // Registrar validadores - Strategy Pattern        builder.Services.AddScoped<IntranetDocumentos.Services.Validation.IReuniaoValidator, IntranetDocumentos.Services.Validation.ReuniaoInternaValidator>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Validation.IReuniaoValidator, IntranetDocumentos.Services.Validation.ReuniaoExternaValidator>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Validation.IReuniaoValidator, IntranetDocumentos.Services.Validation.ReuniaoOnlineValidator>();
        builder.Services.AddScoped<IntranetDocumentos.Services.Validation.IReuniaoValidatorFactory, IntranetDocumentos.Services.Validation.ReuniaoValidatorFactory>();
        
        builder.Services.AddHostedService<BackupBackgroundService>();

        // Configurar logging para Windows Server
        if (OperatingSystem.IsWindows())
        {
#pragma warning disable CA1416 // Validate platform compatibility
            builder.Logging.AddEventLog(settings =>
            {
                settings.SourceName = "IntranetDocumentos";
                settings.LogName = "Application";
            });
#pragma warning restore CA1416 // Validate platform compatibility
        }

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Ativa CORS
        app.UseCors();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // Força HTTPS sempre
        app.UseHttpsRedirection();
        app.UseRouting();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Documents}/{action=Index}/{id?}");

        // Inicialização assíncrona do banco e seed
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var config = app.Configuration;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                // Validar configuração antes de inicializar
                DatabaseInitializer.ValidateConfiguration(config, logger);
                
                // Criar estrutura de diretórios
                DatabaseInitializer.EnsureDataDirectories(config, logger);
                
                // Inicializar banco de dados
                await DatabaseInitializer.InitializeAsync(services);

                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Executar seeding
                await SeedData.Initialize(context, userManager, roleManager, config, logger);
                
                logger.LogInformation("Sistema inicializado com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao inicializar o sistema: {Message}", ex.Message);
                throw;
            }
        }

        await app.RunAsync();
    }
}

public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config,
        ILogger logger)
    {
        // Ensure database is created and migrations are applied
        await context.Database.MigrateAsync();

        // Create roles if they don't exist
        string[] roleNames = { "Admin", "Gestor", "Usuario" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Role {RoleName} criada.", roleName);
            }
        }        // Create default admin user if it doesn't exist
        var adminEmail = config["AdminUser:Email"] ?? "admin@intranet.com";
        var adminPassword = config["AdminUser:Password"] ?? "Admin123!";
        
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            // Find TI department
            var tiDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "TI");
            if (tiDepartment == null)
            {
                tiDepartment = new Department { Name = "TI" };
                context.Departments.Add(tiDepartment);
                await context.SaveChangesAsync();
            }

            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DepartmentId = tiDepartment.Id
            };
            
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Falha ao criar usuário admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new Exception("Falha ao criar usuário admin");
            }
            
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Usuário admin criado com sucesso.");
        }

        // Criar reuniões de exemplo se não existirem
        if (!await context.Reunioes.AnyAsync())
        {
            var today = DateTime.Today;
            var reunioesExemplo = new List<Reuniao>
            {
                new Reuniao
                {
                    Titulo = "Reunião de Planejamento",
                    Data = today.AddDays(4), // Daqui a 4 dias
                    HoraInicio = new TimeSpan(9, 0, 0),
                    HoraFim = new TimeSpan(10, 30, 0),
                    TipoReuniao = TipoReuniao.Interno,
                    Sala = SalaReuniao.Reuniao1,
                    Status = StatusReuniao.Agendada,
                    Observacoes = "Reunião mensal de planejamento",
                    ResponsavelUserId = adminUser.Id,
                    DataCriacao = DateTime.Now
                },
                new Reuniao
                {
                    Titulo = "Reunião com Cliente ABC",
                    Data = today.AddDays(5), // Daqui a 5 dias
                    HoraInicio = new TimeSpan(14, 0, 0),
                    HoraFim = new TimeSpan(16, 0, 0),
                    TipoReuniao = TipoReuniao.Externo,
                    Sala = SalaReuniao.Reuniao2,
                    Veiculo = VeiculoReuniao.Carro1,
                    Empresa = "Cliente ABC Ltda",
                    Status = StatusReuniao.Agendada,
                    Observacoes = "Apresentação de propostas",
                    ResponsavelUserId = adminUser.Id,
                    DataCriacao = DateTime.Now
                },
                new Reuniao
                {
                    Titulo = "Reunião Online - Treinamento",
                    Data = today.AddDays(6), // Daqui a 6 dias
                    HoraInicio = new TimeSpan(10, 0, 0),
                    HoraFim = new TimeSpan(12, 0, 0),
                    TipoReuniao = TipoReuniao.Online,
                    LinkOnline = "https://meet.google.com/abc-defg-hij",
                    Status = StatusReuniao.Agendada,
                    Observacoes = "Treinamento da equipe",
                    ResponsavelUserId = adminUser.Id,
                    DataCriacao = DateTime.Now
                },
                new Reuniao
                {
                    Titulo = "Reunião de Status Semanal",
                    Data = today, // Hoje
                    HoraInicio = new TimeSpan(15, 0, 0),
                    HoraFim = new TimeSpan(16, 0, 0),
                    TipoReuniao = TipoReuniao.Interno,
                    Sala = SalaReuniao.Diretoria,
                    Status = StatusReuniao.Agendada,
                    Observacoes = "Status semanal dos projetos",
                    ResponsavelUserId = adminUser.Id,
                    DataCriacao = DateTime.Now
                },
                new Reuniao
                {
                    Titulo = "Reunião com Fornecedor XYZ",
                    Data = today.AddDays(1), // Amanhã
                    HoraInicio = new TimeSpan(11, 0, 0),
                    HoraFim = new TimeSpan(12, 30, 0),
                    TipoReuniao = TipoReuniao.Externo,
                    Sala = SalaReuniao.Reuniao1,
                    Veiculo = VeiculoReuniao.Van,
                    Empresa = "Fornecedor XYZ S/A",
                    Status = StatusReuniao.Agendada,
                    Observacoes = "Negociação de contrato",
                    ResponsavelUserId = adminUser.Id,
                    DataCriacao = DateTime.Now
                }
            };

            context.Reunioes.AddRange(reunioesExemplo);
            await context.SaveChangesAsync();

            // Adicionar alguns participantes para as reuniões
            var tiDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "TI");
            var participantesExemplo = new List<ReuniaoParticipante>
            {
                new ReuniaoParticipante { ReuniaoId = reunioesExemplo[0].Id, Nome = "João Silva", DepartamentoId = tiDepartment?.Id },
                new ReuniaoParticipante { ReuniaoId = reunioesExemplo[0].Id, Nome = "Maria Santos", DepartamentoId = tiDepartment?.Id },
                new ReuniaoParticipante { ReuniaoId = reunioesExemplo[1].Id, Nome = "Pedro Costa", DepartamentoId = tiDepartment?.Id },
                new ReuniaoParticipante { ReuniaoId = reunioesExemplo[2].Id, Nome = "Ana Oliveira", DepartamentoId = tiDepartment?.Id },
                new ReuniaoParticipante { ReuniaoId = reunioesExemplo[3].Id, Nome = "Carlos Ferreira", DepartamentoId = tiDepartment?.Id },
                new ReuniaoParticipante { ReuniaoId = reunioesExemplo[4].Id, Nome = "Lucia Rodrigues", DepartamentoId = tiDepartment?.Id }
            };

            context.ReuniaoParticipantes.AddRange(participantesExemplo);
        }

        // Create sample documents and download logs for analytics if they don't exist
        if (!await context.Documents.AnyAsync())
        {
            // Ensure departments exist
            await EnsureDepartmentsExistAsync(context);
            
            var tiDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "TI");
            var pessoalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Pessoal");
            var fiscalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Fiscal");
            var geralDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Geral");

            var sampleDocuments = new List<Document>
            {
                new Document
                {
                    OriginalFileName = "manual_sistema.pdf",
                    StoredFileName = $"manual_{Guid.NewGuid()}.pdf",
                    ContentType = "application/pdf",
                    FileSize = 1024000,
                    DepartmentId = tiDepartment?.Id,
                    UploaderId = adminUser.Id,
                    UploadDate = DateTime.Now.AddDays(-30)
                },
                new Document
                {
                    OriginalFileName = "politica_rh.pdf",
                    StoredFileName = $"politica_{Guid.NewGuid()}.pdf",
                    ContentType = "application/pdf",
                    FileSize = 512000,
                    DepartmentId = pessoalDepartment?.Id,
                    UploaderId = adminUser.Id,
                    UploadDate = DateTime.Now.AddDays(-20)
                },
                new Document
                {
                    OriginalFileName = "relatorio_fiscal.xlsx",
                    StoredFileName = $"relatorio_{Guid.NewGuid()}.xlsx",
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileSize = 256000,
                    DepartmentId = fiscalDepartment?.Id,
                    UploaderId = adminUser.Id,
                    UploadDate = DateTime.Now.AddDays(-10)
                },
                new Document
                {
                    OriginalFileName = "comunicado.pdf",
                    StoredFileName = $"comunicado_{Guid.NewGuid()}.pdf",
                    ContentType = "application/pdf",
                    FileSize = 128000,
                    DepartmentId = geralDepartment?.Id,
                    UploaderId = adminUser.Id,
                    UploadDate = DateTime.Now.AddDays(-5)
                }
            };

            context.Documents.AddRange(sampleDocuments);
            await context.SaveChangesAsync();

            // Create sample download logs for analytics
            var downloadLogs = new List<DocumentDownloadLog>();
            
            foreach (var doc in sampleDocuments)
            {
                // Add multiple downloads for each document at different dates
                for (int i = 1; i <= 3; i++)
                {
                    downloadLogs.Add(new DocumentDownloadLog
                    {
                        DocumentId = doc.Id,
                        UserId = adminUser.Id,
                        DownloadDate = DateTime.Now.AddDays(-i * 3),
                        UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/91.{i}",
                        IpAddress = $"192.168.1.{100 + i}"
                    });
                }
            }

            context.DocumentDownloadLogs.AddRange(downloadLogs);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Sample documents and download logs created for analytics");
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDepartmentsExistAsync(ApplicationDbContext context)
    {
        var departments = new[] { "TI", "Pessoal", "Fiscal", "Contábil", "Cadastro", "Apoio", "Geral" };
        
        foreach (var deptName in departments)
        {
            if (!await context.Departments.AnyAsync(d => d.Name == deptName))
            {
                context.Departments.Add(new Department 
                {
                    Name = deptName
                });
            }
        }
        
        await context.SaveChangesAsync();
    }
}

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Aplicar migrations pendentes
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Aplicando migrations pendentes...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations aplicadas com sucesso.");
            }
            else
            {
                logger.LogInformation("Banco de dados está atualizado.");
            }

            // Verificar se o banco está acessível
            await context.Database.CanConnectAsync();
            logger.LogInformation("Conexão com banco de dados verificada com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inicializar banco de dados: {Message}", ex.Message);
            throw;
        }
    }

    public static void EnsureDataDirectories(IConfiguration configuration, ILogger logger)
    {
        try
        {
            var documentsPath = configuration["DocumentSettings:StoragePath"] ?? "DocumentsStorage";
            var backupPath = configuration["DocumentSettings:BackupPath"] ?? "DatabaseBackups";

            // Converter caminhos relativos para absolutos se necessário
            if (!Path.IsPathRooted(documentsPath))
            {
                documentsPath = Path.Combine(Directory.GetCurrentDirectory(), documentsPath);
            }

            if (!Path.IsPathRooted(backupPath))
            {
                backupPath = Path.Combine(Directory.GetCurrentDirectory(), backupPath);
            }

            // Criar diretórios se não existirem
            var directories = new[]
            {
                documentsPath,
                backupPath,
                Path.Combine(documentsPath, "Geral"),
                Path.Combine(documentsPath, "Pessoal"),
                Path.Combine(documentsPath, "Fiscal"),
                Path.Combine(documentsPath, "Contabil"),
                Path.Combine(documentsPath, "Cadastro"),
                Path.Combine(documentsPath, "Apoio"),
                Path.Combine(documentsPath, "TI"),
                Path.Combine(backupPath, "Auto")
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    logger.LogInformation("Diretório criado: {Directory}", dir);
                }
            }

            logger.LogInformation("Estrutura de diretórios verificada com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao criar estrutura de diretórios: {Message}", ex.Message);
            throw;
        }
    }

    public static void ValidateConfiguration(IConfiguration configuration, ILogger logger)
    {
        var errors = new List<string>();

        // Validar connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            errors.Add("Connection string 'DefaultConnection' não configurada");
        }
        else if (connectionString.Contains("CHANGE_THIS_PASSWORD"))
        {
            errors.Add("Connection string contém senha padrão - configure uma senha segura");
        }

        // Validar configurações de email (opcional)
        var emailConfig = configuration.GetSection("EmailSettings");
        if (emailConfig.Exists())
        {
            var smtpPassword = emailConfig["SmtpPassword"];
            if (!string.IsNullOrEmpty(smtpPassword) && smtpPassword.Contains("CHANGE_THIS"))
            {
                logger.LogWarning("Configuração de email contém senha padrão - emails não funcionarão");
            }
        }

        // Validar paths
        var documentsPath = configuration["DocumentSettings:StoragePath"];
        if (!string.IsNullOrEmpty(documentsPath) && !Directory.Exists(Path.GetDirectoryName(documentsPath)))
        {
            errors.Add($"Diretório pai para documentos não existe: {Path.GetDirectoryName(documentsPath)}");
        }

        if (errors.Any())
        {
            var errorMessage = "Erros de configuração encontrados:\n" + string.Join("\n", errors);
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        logger.LogInformation("Configuração validada com sucesso.");
    }
}
