using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IntranetDocumentos.Models.ValueObjects
{
    /// <summary>
    /// Value Object que representa um status/estado com validações
    /// </summary>
    public sealed class StatusValue : ValueObject
    {
        public string Value { get; }
        public string DisplayName { get; }
        public bool IsActive { get; }

        public StatusValue(string value, string displayName, bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Status não pode ser vazio ou nulo", nameof(value));

            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Nome de exibição não pode ser vazio ou nulo", nameof(displayName));

            if (value.Length > 50)
                throw new ArgumentException("Status não pode ter mais de 50 caracteres", nameof(value));

            if (displayName.Length > 100)
                throw new ArgumentException("Nome de exibição não pode ter mais de 100 caracteres", nameof(displayName));

            Value = value.Trim();
            DisplayName = displayName.Trim();
            IsActive = isActive;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => DisplayName;

        // Conversões implícitas
        public static implicit operator string(StatusValue status) => status.Value;

        // Factory methods
        public static StatusValue Create(string value, string displayName, bool isActive = true) 
            => new(value, displayName, isActive);

        public static bool TryCreate(string value, string displayName, out StatusValue? status, bool isActive = true)
        {
            status = null;
            try
            {
                status = new StatusValue(value, displayName, isActive);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Status pré-definidos para documentos
        public static readonly StatusValue Draft = Create("draft", "Rascunho");
        public static readonly StatusValue PendingReview = Create("pending_review", "Aguardando Revisão");
        public static readonly StatusValue InReview = Create("in_review", "Em Revisão");
        public static readonly StatusValue PendingApproval = Create("pending_approval", "Aguardando Aprovação");
        public static readonly StatusValue Approved = Create("approved", "Aprovado");
        public static readonly StatusValue Rejected = Create("rejected", "Rejeitado");
        public static readonly StatusValue ChangesRequested = Create("changes_requested", "Alterações Solicitadas");
        public static readonly StatusValue Published = Create("published", "Publicado");
        public static readonly StatusValue Archived = Create("archived", "Arquivado", false);

        // Métodos helper
        public bool IsValidTransition(StatusValue newStatus)
        {
            // Define regras de transição de status
            return Value switch
            {
                "draft" => newStatus.Value is "pending_review" or "archived",
                "pending_review" => newStatus.Value is "in_review" or "rejected" or "draft",
                "in_review" => newStatus.Value is "pending_approval" or "changes_requested" or "rejected",
                "pending_approval" => newStatus.Value is "approved" or "rejected" or "changes_requested",
                "approved" => newStatus.Value is "published" or "archived",
                "rejected" => newStatus.Value is "draft" or "archived",
                "changes_requested" => newStatus.Value is "draft" or "archived",
                "published" => newStatus.Value is "archived",
                "archived" => newStatus.Value is "draft", // Permite reativar
                _ => false
            };
        }

        public bool CanBeModified()
        {
            return Value is "draft" or "changes_requested";
        }

        public bool RequiresApproval()
        {
            return Value is "pending_review" or "in_review" or "pending_approval";
        }

        public bool IsCompleted()
        {
            return Value is "published" or "archived";
        }
    }
}
