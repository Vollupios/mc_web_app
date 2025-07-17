using IntranetDocumentos.Models;
using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ViewModels
{
    /// <summary>
    /// ViewModel para exibição hierárquica de pastas e documentos
    /// </summary>
    public class DocumentTreeViewModel
    {
        public List<DocumentFolderTreeNode> RootFolders { get; set; } = new();
        public List<DocumentFolderTreeNode> FolderTree { get; set; } = new();
        public List<Document> RootDocuments { get; set; } = new();
        public DocumentFolder? CurrentFolder { get; set; }
        public List<DocumentFolder> Breadcrumbs { get; set; } = new();
        public FolderNavigationViewModel? Navigation { get; set; }
        public int? CurrentDepartmentId { get; set; }
        public string? SearchTerm { get; set; }
        public bool CanCreateFolders { get; set; }
        public bool CanUpload { get; set; }
    }

    /// <summary>
    /// Representa um nó na árvore de pastas
    /// </summary>
    public class DocumentFolderTreeNode
    {
        public DocumentFolder Folder { get; set; } = null!;
        public List<DocumentFolderTreeNode> Children { get; set; } = new();
        public List<Document> Documents { get; set; } = new();
        public bool IsExpanded { get; set; } = false;
        public bool IsSelected { get; set; } = false;
        public int TotalDocuments { get; set; }
        public int DirectDocuments { get; set; }
        public int DocumentsCount { get; set; }
        public int SubFoldersCount { get; set; }
        
        /// <summary>
        /// Converte uma lista flat de pastas em uma árvore hierárquica
        /// </summary>
        public static List<DocumentFolderTreeNode> BuildTree(List<DocumentFolder> folders, List<Document> documents)
        {
            var folderNodes = folders.ToDictionary(f => f.Id, f => new DocumentFolderTreeNode 
            { 
                Folder = f,
                Documents = documents.Where(d => d.FolderId == f.Id).ToList()
            });

            var rootNodes = new List<DocumentFolderTreeNode>();

            foreach (var folder in folders.OrderBy(f => f.DisplayOrder).ThenBy(f => f.Name))
            {
                var node = folderNodes[folder.Id];
                node.DirectDocuments = node.Documents.Count;

                if (folder.ParentFolderId == null)
                {
                    rootNodes.Add(node);
                }
                else if (folderNodes.ContainsKey(folder.ParentFolderId.Value))
                {
                    folderNodes[folder.ParentFolderId.Value].Children.Add(node);
                }
            }

            // Calcular totais recursivamente
            foreach (var root in rootNodes)
            {
                CalculateTotals(root);
            }

            return rootNodes;
        }

        private static int CalculateTotals(DocumentFolderTreeNode node)
        {
            node.TotalDocuments = node.DirectDocuments;
            
            foreach (var child in node.Children)
            {
                node.TotalDocuments += CalculateTotals(child);
            }

            return node.TotalDocuments;
        }
    }

    /// <summary>
    /// ViewModel para navegação de pastas
    /// </summary>
    public class FolderNavigationViewModel
    {
        public int? CurrentFolderId { get; set; }
        public DocumentFolder? CurrentFolder { get; set; }
        public List<DocumentFolder> Breadcrumbs { get; set; } = new();
        public List<DocumentFolder> SubFolders { get; set; } = new();
        public List<Document> Documents { get; set; } = new();
        public int? DepartmentId { get; set; }
        public string? SearchTerm { get; set; }
        
        // Permissões
        public bool CanCreateFolder { get; set; }
        public bool CanEditFolder { get; set; }
        public bool CanDeleteFolder { get; set; }
        public bool CanUploadDocument { get; set; }
        
        // Estatísticas
        public int TotalFolders { get; set; }
        public int TotalDocuments { get; set; }
        public long TotalSize { get; set; }
        
        public string FormattedTotalSize => FormatBytes(TotalSize);
        
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    /// <summary>
    /// ViewModel para criação/edição de pastas
    /// </summary>
    public class FolderFormViewModel
    {
        public int? Id { get; set; }
        
        [Required(ErrorMessage = "Nome da pasta é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }
        
        public int? ParentFolderId { get; set; }
        public int? DepartmentId { get; set; }
        public string Color { get; set; } = "#007bff";
        public string Icon { get; set; } = "bi-folder";
        public int DisplayOrder { get; set; }
        public bool IsSystemFolder { get; set; }
        
        // Para dropdowns
        public List<DocumentFolder> AvailableParentFolders { get; set; } = new();
        public List<Department> AvailableDepartments { get; set; } = new();
        
        // Contexto
        public bool IsEdit => Id.HasValue;
        public string Title => IsEdit ? "Editar Pasta" : "Nova Pasta";
        public string SubmitButtonText => IsEdit ? "Salvar Alterações" : "Criar Pasta";
    }

    /// <summary>
    /// ViewModel para ações em lote em pastas
    /// </summary>
    public class FolderBulkActionViewModel
    {
        public List<int> SelectedFolderIds { get; set; } = new();
        public string Action { get; set; } = string.Empty; // move, delete, archive
        public int? TargetFolderId { get; set; }
        public int? TargetDepartmentId { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// ViewModel para mover documentos/pastas
    /// </summary>
    public class MoveItemsViewModel
    {
        public List<int> DocumentIds { get; set; } = new();
        public List<int> FolderIds { get; set; } = new();
        public int? TargetFolderId { get; set; }
        public int? CurrentFolderId { get; set; }
        
        public List<DocumentFolderTreeNode> FolderTree { get; set; } = new();
        public string? Reason { get; set; }
        
        public int TotalItems => DocumentIds.Count + FolderIds.Count;
        public string ItemsDescription
        {
            get
            {
                var parts = new List<string>();
                if (DocumentIds.Count > 0)
                    parts.Add($"{DocumentIds.Count} documento(s)");
                if (FolderIds.Count > 0)
                    parts.Add($"{FolderIds.Count} pasta(s)");
                return string.Join(" e ", parts);
            }
        }
    }

    /// <summary>
    /// ViewModel para breadcrumbs de navegação
    /// </summary>
    public class BreadcrumbItem
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-folder";
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// ViewModel para mover documento entre pastas e departamentos
    /// </summary>
    public class MoveDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public int? CurrentDepartmentId { get; set; }
        public int? CurrentFolderId { get; set; }
        public int? NewDepartmentId { get; set; }
        public int? NewFolderId { get; set; }
        public List<Department> Departments { get; set; } = new();
        public List<DocumentFolder> AvailableFolders { get; set; } = new();
        public string? Reason { get; set; } // Motivo da movimentação (para auditoria)
    }

    /// <summary>
    /// ViewModel para movimentação em lote de documentos
    /// </summary>
    public class BulkMoveDocumentViewModel
    {
        public int[] DocumentIds { get; set; } = Array.Empty<int>();
        public int? NewDepartmentId { get; set; }
        public int? NewFolderId { get; set; }
        public string? Reason { get; set; }
        public List<Department> Departments { get; set; } = new();
        public List<DocumentFolder> AvailableFolders { get; set; } = new();
    }
}
