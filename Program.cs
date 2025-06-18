using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IntranetDocumentos.Data;
using IntranetDocumentos.Models;
using IntranetDocumentos.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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
    options.UseSqlite(connectionString));

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
// Inicializa o banco de dados e cria os roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = app.Configuration;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        await SeedData.Initialize(context, userManager, roleManager, config, logger);
        logger.LogInformation("Seed da base de dados concluído com sucesso");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao inicializar o banco de dados.");
        // Não relançamos a exceção aqui para permitir que a aplicação continue mesmo se o seed falhar
    }
}

app.Run();

public static class SeedData
{
    public static async Task Initialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config,
        ILogger logger)
    {        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create Ramais table if it doesn't exist
        await CreateRamaisTableIfNotExists(context);

        // Create roles if they don't exist
        string[] roleNames = { "Admin", "Gestor", "Usuario" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create default admin user if it doesn't exist
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
            // Não precisa de SaveChangesAsync aqui, pois Identity já salva.
        }        await context.SaveChangesAsync();
    }    /// <summary>
    /// Cria a tabela Ramais se ela não existir
    /// </summary>
    private static async Task CreateRamaisTableIfNotExists(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Ramais (
                Id INTEGER NOT NULL CONSTRAINT PK_Ramais PRIMARY KEY AUTOINCREMENT,
                Numero TEXT NOT NULL,
                Nome TEXT NOT NULL,
                TipoFuncionario INTEGER NOT NULL DEFAULT 0,
                DepartmentId INTEGER NULL,
                FotoPath TEXT NULL,
                Observacoes TEXT NULL,
                DataCriacao TEXT NOT NULL,
                Ativo INTEGER NOT NULL,
                CONSTRAINT FK_Ramais_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Departments (Id) ON DELETE SET NULL
            )";
        await command.ExecuteNonQueryAsync();

        // Adicionar campo TipoFuncionario se a tabela já existir mas não tiver este campo
        try
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = "ALTER TABLE Ramais ADD COLUMN TipoFuncionario INTEGER NOT NULL DEFAULT 0";
            await alterCommand.ExecuteNonQueryAsync();
        }
        catch
        {
            // Campo já existe, ignora o erro
        }

        await connection.CloseAsync();
    }
}
