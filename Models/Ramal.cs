using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    /// <summary>
    /// Modelo para representar os ramais telefônicos do escritório
    /// </summary>
    public class Ramal
    {
        /// <summary>
        /// Identificador único do ramal
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número do ramal telefônico
        /// </summary>        [Display(Name = "Número do Ramal")]
        [StringLength(10, ErrorMessage = "O número do ramal deve ter no máximo 10 caracteres")]
        public string Numero { get; set; } = string.Empty;

        /// <summary>
        /// Nome da pessoa responsável pelo ramal
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de funcionário (Normal, Licença Maternidade, Externo)
        /// </summary>
        [Required(ErrorMessage = "O tipo de funcionário é obrigatório")]
        [Display(Name = "Tipo de Funcionário")]
        public TipoFuncionario TipoFuncionario { get; set; } = TipoFuncionario.Normal;

        /// <summary>
        /// Departamento ao qual o ramal pertence
        /// </summary>
        [Display(Name = "Departamento")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Navegação para o departamento
        /// </summary>
        public Department? Department { get; set; }

        /// <summary>
        /// Caminho para a foto da pessoa (opcional)
        /// </summary>
        [Display(Name = "Foto")]
        [StringLength(200, ErrorMessage = "O caminho da foto deve ter no máximo 200 caracteres")]
        public string? FotoPath { get; set; }

        /// <summary>
        /// Observações adicionais sobre o ramal
        /// </summary>
        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Observacoes { get; set; }

        /// <summary>
        /// Data de criação do registro
        /// </summary>
        [Display(Name = "Data de Cadastro")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        /// <summary>
        /// Indica se o ramal está ativo
        /// </summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;        /// <summary>
        /// Propriedade computada que retorna o número do ramal ou status correspondente
        /// </summary>
        [Display(Name = "Ramal/Status")]
        public string RamalDisplay => TipoFuncionario switch
        {
            TipoFuncionario.Normal => Numero,
            TipoFuncionario.LicencaMaternidade => "Indisponível",
            TipoFuncionario.Externo => "Externo",
            _ => "Externo"
        };
    }

    /// <summary>
    /// Enum para tipos de funcionário
    /// </summary>
    public enum TipoFuncionario
    {
        [Display(Name = "Normal")]
        Normal = 0,
        
        [Display(Name = "Licença Maternidade")]
        LicencaMaternidade = 1,
        
        [Display(Name = "Externo")]
        Externo = 2
    }
}
