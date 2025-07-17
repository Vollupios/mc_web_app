using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models
{
    /// <summary>
    /// Representa uma pasta ou subpasta no sistema de documentos
    /// Permite hierarquia de pastas com navegação em árvore
    /// </summary>
    public class DocumentFolder
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// ID da pasta pai (null para pastas raiz)
        /// </summary>
        public int? ParentFolderId { get; set; }
        
        /// <summary>
        /// Departamento ao qual a pasta pertence (null para pastas globais)
        /// </summary>
        public int? DepartmentId { get; set; }
        
        /// <summary>
        /// Cor da pasta para interface visual
        /// </summary>
        [StringLength(7)]
        public string Color { get; set; } = "#007bff"; // Azul padrão
        
        /// <summary>
        /// Ícone da pasta (Bootstrap Icons)
        /// </summary>
        [StringLength(50)]
        public string Icon { get; set; } = "bi-folder";
        
        /// <summary>
        /// Ordem de exibição dentro da pasta pai
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Caminho completo da pasta (ex: "Pasta1/Subpasta1/Subpasta2")
        /// </summary>
        [StringLength(1000)]
        public string Path { get; set; } = string.Empty;
        
        /// <summary>
        /// Nível hierárquico (0 = raiz)
        /// </summary>
        public int Level { get; set; } = 0;
        
        /// <summary>
        /// Se a pasta é do sistema (não pode ser deletada)
        /// </summary>
        public bool IsSystemFolder { get; set; } = false;
        
        /// <summary>
        /// Se a pasta está ativa
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string CreatedById { get; set; } = string.Empty;
        
        public string? UpdatedById { get; set; }
        
        // Navigation Properties
        public virtual DocumentFolder? ParentFolder { get; set; }
        public virtual ICollection<DocumentFolder> ChildFolders { get; set; } = new List<DocumentFolder>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual Department? Department { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual ApplicationUser? UpdatedBy { get; set; }
        
        // Computed Properties
        public bool HasChildren => ChildFolders.Any();
        public int DocumentCount => Documents.Count;
        public int TotalDocumentCount => Documents.Count + ChildFolders.Sum(f => f.TotalDocumentCount);
        
        /// <summary>
        /// Constrói o caminho completo da pasta
        /// </summary>
        public void BuildPath()
        {
            if (ParentFolder != null)
            {
                ParentFolder.BuildPath();
                Path = $"{ParentFolder.Path}/{Name}";
                Level = ParentFolder.Level + 1;
            }
            else
            {
                Path = Name;
                Level = 0;
            }
        }
        
        /// <summary>
        /// Obtém todas as subpastas recursivamente
        /// </summary>
        public List<DocumentFolder> GetAllDescendants()
        {
            var result = new List<DocumentFolder>();
            
            foreach (var child in ChildFolders.Where(f => f.IsActive))
            {
                result.Add(child);
                result.AddRange(child.GetAllDescendants());
            }
            
            return result;
        }
        
        /// <summary>
        /// Obtém todos os ancestrais (caminho até a raiz)
        /// </summary>
        public List<DocumentFolder> GetAncestors()
        {
            var result = new List<DocumentFolder>();
            var current = ParentFolder;
            
            while (current != null)
            {
                result.Insert(0, current);
                current = current.ParentFolder;
            }
            
            return result;
        }
        
        /// <summary>
        /// Verifica se pode ser movida para outra pasta
        /// </summary>
        public bool CanMoveTo(DocumentFolder? targetFolder)
        {
            if (targetFolder == null) return true;
            if (targetFolder.Id == Id) return false;
            
            // Não pode mover para uma subpasta própria
            var descendants = GetAllDescendants();
            return !descendants.Any(d => d.Id == targetFolder.Id);
        }
        
        /// <summary>
        /// Gera um nome único para subpasta
        /// </summary>
        public string GenerateUniqueChildName(string baseName)
        {
            var existingNames = ChildFolders.Select(f => f.Name).ToHashSet();
            
            if (!existingNames.Contains(baseName))
                return baseName;
                
            int counter = 1;
            string uniqueName;
            
            do
            {
                uniqueName = $"{baseName} ({counter})";
                counter++;
            }
            while (existingNames.Contains(uniqueName));
            
            return uniqueName;
        }
        
        // Propriedades computadas para Views
        public int DocumentsCount => Documents?.Count ?? 0;
        public int SubFoldersCount => ChildFolders?.Count ?? 0;
        public int FolderId => Id; // Propriedade alias para compatibilidade com Views
    }
}
