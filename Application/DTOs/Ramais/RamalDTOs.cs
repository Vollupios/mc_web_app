using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.DTOs.Ramais
{
    /// <summary>
    /// DTO para criar ramal
    /// </summary>
    public class CreateRamalDTO
    {
        [Required(ErrorMessage = "O número do ramal é obrigatório")]
        [StringLength(10, ErrorMessage = "O número do ramal deve ter no máximo 10 caracteres")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de funcionário é obrigatório")]
        public TipoFuncionario TipoFuncionario { get; set; } = TipoFuncionario.Normal;

        public int? DepartmentId { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        public bool Ativo { get; set; } = true;

        public IFormFile? FotoFile { get; set; }
    }

    /// <summary>
    /// DTO para atualização de ramal
    /// </summary>
    public class UpdateRamalDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "O número do ramal é obrigatório")]
        [StringLength(10, ErrorMessage = "O número do ramal deve ter no máximo 10 caracteres")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de funcionário é obrigatório")]
        public TipoFuncionario TipoFuncionario { get; set; } = TipoFuncionario.Normal;

        public int? DepartmentId { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        public bool Ativo { get; set; } = true;

        public IFormFile? FotoFile { get; set; }
        public string? FotoPath { get; set; }
    }

    /// <summary>
    /// DTO para resposta de ramal
    /// </summary>
    public class RamalDTO : BaseDTO
    {
        public string Numero { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public TipoFuncionario TipoFuncionario { get; set; }
        public string TipoFuncionarioDescricao => TipoFuncionario.GetDescription();
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? Observacoes { get; set; }
        public bool Ativo { get; set; }
        public string? FotoPath { get; set; }
        public string? FotoUrl { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }

    /// <summary>
    /// DTO para filtros de busca de ramais
    /// </summary>
    public class RamalSearchDTO
    {
        public string? Query { get; set; }
        public string? Numero { get; set; }
        public string? Nome { get; set; }
        public int? DepartmentId { get; set; }
        public TipoFuncionario? TipoFuncionario { get; set; }
        public bool? Ativo { get; set; }
        
        // Paginação
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Ordenação
        public string? SortBy { get; set; } = "Nome";
        public string? SortOrder { get; set; } = "asc";
    }

    /// <summary>
    /// DTO para estatísticas de ramais
    /// </summary>
    public class RamalStatisticsDTO
    {
        public int TotalRamais { get; set; }
        public int RamaisAtivos { get; set; }
        public int RamaisInativos { get; set; }
        public int RamaisComFoto { get; set; }
        public int RamaisSemFoto { get; set; }
        
        public List<RamalsByDepartmentDTO> RamaisByDepartment { get; set; } = new();
        public List<RamalsByTypeDTO> RamaisByType { get; set; } = new();
        public List<RamalsByStatusDTO> RamaisByStatus { get; set; } = new();
    }

    /// <summary>
    /// DTO para ramais por departamento
    /// </summary>
    public class RamalsByDepartmentDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para ramais por tipo
    /// </summary>
    public class RamalsByTypeDTO
    {
        public TipoFuncionario TipoFuncionario { get; set; }
        public string TipoFuncionarioDescricao => TipoFuncionario.GetDescription();
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para ramais por status
    /// </summary>
    public class RamalsByStatusDTO
    {
        public bool Ativo { get; set; }
        public string StatusDescricao => Ativo ? "Ativo" : "Inativo";
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO para upload de foto de ramal
    /// </summary>
    public class RamalFotoUploadDTO
    {
        [Required]
        public int RamalId { get; set; }

        [Required(ErrorMessage = "O arquivo de foto é obrigatório")]
        public IFormFile FotoFile { get; set; } = null!;
    }

    /// <summary>
    /// DTO para resposta de upload de foto
    /// </summary>
    public class RamalFotoResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FotoPath { get; set; }
        public string? FotoUrl { get; set; }
    }

    /// <summary>
    /// Enum para tipo de funcionário
    /// </summary>
    public enum TipoFuncionario
    {
        Normal = 1,
        Gerente = 2,
        Diretor = 3,
        Presidente = 4,
        Terceirizado = 5,
        Estagiario = 6,
        Temporario = 7
    }

    /// <summary>
    /// Extensões para o enum TipoFuncionario
    /// </summary>
    public static class TipoFuncionarioExtensions
    {
        public static string GetDescription(this TipoFuncionario tipo)
        {
            return tipo switch
            {
                TipoFuncionario.Normal => "Normal",
                TipoFuncionario.Gerente => "Gerente",
                TipoFuncionario.Diretor => "Diretor",
                TipoFuncionario.Presidente => "Presidente",
                TipoFuncionario.Terceirizado => "Terceirizado",
                TipoFuncionario.Estagiario => "Estagiário",
                TipoFuncionario.Temporario => "Temporário",
                _ => "Desconhecido"
            };
        }
    }
}
