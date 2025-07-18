using System.ComponentModel.DataAnnotations;
using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.Factories
{
    /// <summary>
    /// Implementação base para factories
    /// Fornece funcionalidades comuns para criação e validação
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade a ser criada</typeparam>
    /// <typeparam name="TCreateDto">Tipo do DTO de criação</typeparam>
    public abstract class BaseFactory<TEntity, TCreateDto> : IFactory<TEntity, TCreateDto>
        where TEntity : class
        where TCreateDto : BaseDTO
    {
        /// <summary>
        /// Cria uma nova instância da entidade a partir do DTO
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        public virtual TEntity Create(TCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (!IsValid(createDto, out var errors))
                throw new ValidationException($"DTO inválido: {string.Join(", ", errors)}");

            return CreateEntity(createDto);
        }

        /// <summary>
        /// Cria uma nova instância da entidade com validação assíncrona
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade validada</returns>
        public virtual async Task<TEntity> CreateAsync(TCreateDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (!IsValid(createDto, out var errors))
                throw new ValidationException($"DTO inválido: {string.Join(", ", errors)}");

            await ValidateAsync(createDto);

            return CreateEntity(createDto);
        }

        /// <summary>
        /// Valida se o DTO é válido para criação
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>True se válido, False caso contrário</returns>
        public virtual bool IsValid(TCreateDto createDto)
        {
            return IsValid(createDto, out _);
        }

        /// <summary>
        /// Valida se o DTO é válido para criação com mensagens de erro
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <param name="errors">Lista de erros encontrados</param>
        /// <returns>True se válido, False caso contrário</returns>
        public virtual bool IsValid(TCreateDto createDto, out List<string> errors)
        {
            errors = new List<string>();

            if (createDto == null)
            {
                errors.Add("DTO não pode ser nulo");
                return false;
            }

            // Validação usando Data Annotations
            var validationContext = new ValidationContext(createDto);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(createDto, validationContext, validationResults, true))
            {
                errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Erro de validação"));
            }

            // Validações customizadas
            var customErrors = ValidateCustom(createDto);
            if (customErrors.Any())
            {
                errors.AddRange(customErrors);
            }

            return !errors.Any();
        }

        /// <summary>
        /// Método abstrato para criação da entidade
        /// Deve ser implementado pelas classes filhas
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        protected abstract TEntity CreateEntity(TCreateDto createDto);

        /// <summary>
        /// Método virtual para validações customizadas
        /// Pode ser sobrescrito pelas classes filhas
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>Lista de erros customizados</returns>
        protected virtual List<string> ValidateCustom(TCreateDto createDto)
        {
            return new List<string>();
        }

        /// <summary>
        /// Método virtual para validações assíncronas
        /// Pode ser sobrescrito pelas classes filhas
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>Task de validação</returns>
        protected virtual Task ValidateAsync(TCreateDto createDto)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Método auxiliar para definir valores padrão
        /// </summary>
        /// <param name="entity">Entidade a ser configurada</param>
        /// <param name="createDto">DTO com dados</param>
        protected virtual void SetDefaults(TEntity entity, TCreateDto createDto)
        {
            // Implementação padrão vazia
            // Pode ser sobrescrita pelas classes filhas
        }

        /// <summary>
        /// Método auxiliar para aplicar transformações
        /// </summary>
        /// <param name="entity">Entidade a ser transformada</param>
        /// <param name="createDto">DTO com dados</param>
        protected virtual void ApplyTransformations(TEntity entity, TCreateDto createDto)
        {
            // Implementação padrão vazia
            // Pode ser sobrescrita pelas classes filhas
        }

        /// <summary>
        /// Método auxiliar para configurações finais
        /// </summary>
        /// <param name="entity">Entidade a ser configurada</param>
        /// <param name="createDto">DTO com dados</param>
        protected virtual void FinalizeEntity(TEntity entity, TCreateDto createDto)
        {
            // Implementação padrão vazia
            // Pode ser sobrescrita pelas classes filhas
        }
    }

    /// <summary>
    /// Factory base para criação de entidades com timestamp
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade a ser criada</typeparam>
    /// <typeparam name="TCreateDto">Tipo do DTO de criação</typeparam>
    public abstract class TimestampedFactory<TEntity, TCreateDto> : BaseFactory<TEntity, TCreateDto>
        where TEntity : class
        where TCreateDto : BaseDTO
    {
        /// <summary>
        /// Cria uma nova instância da entidade com timestamps
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        public override TEntity Create(TCreateDto createDto)
        {
            var entity = base.Create(createDto);
            SetTimestamps(entity);
            return entity;
        }

        /// <summary>
        /// Cria uma nova instância da entidade com timestamps (assíncrono)
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        public override async Task<TEntity> CreateAsync(TCreateDto createDto)
        {
            var entity = await base.CreateAsync(createDto);
            SetTimestamps(entity);
            return entity;
        }

        /// <summary>
        /// Define os timestamps na entidade
        /// </summary>
        /// <param name="entity">Entidade a ser configurada</param>
        protected virtual void SetTimestamps(TEntity entity)
        {
            var now = DateTime.UtcNow;
            
            // Usar reflection para definir propriedades de timestamp
            var entityType = typeof(TEntity);
            
            var createdAtProperty = entityType.GetProperty("CreatedAt");
            createdAtProperty?.SetValue(entity, now);
            
            var updatedAtProperty = entityType.GetProperty("UpdatedAt");
            updatedAtProperty?.SetValue(entity, now);
            
            var dataCriacaoProperty = entityType.GetProperty("DataCriacao");
            dataCriacaoProperty?.SetValue(entity, now);
        }
    }

    /// <summary>
    /// Factory base para criação de entidades com usuário
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade a ser criada</typeparam>
    /// <typeparam name="TCreateDto">Tipo do DTO de criação</typeparam>
    public abstract class UserContextFactory<TEntity, TCreateDto> : TimestampedFactory<TEntity, TCreateDto>
        where TEntity : class
        where TCreateDto : BaseDTO
    {
        protected readonly string _currentUserId;

        /// <summary>
        /// Construtor com contexto do usuário
        /// </summary>
        /// <param name="currentUserId">ID do usuário atual</param>
        protected UserContextFactory(string currentUserId)
        {
            _currentUserId = currentUserId ?? throw new ArgumentNullException(nameof(currentUserId));
        }

        /// <summary>
        /// Cria uma nova instância da entidade com contexto do usuário
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        public override TEntity Create(TCreateDto createDto)
        {
            var entity = base.Create(createDto);
            SetUserContext(entity);
            return entity;
        }

        /// <summary>
        /// Cria uma nova instância da entidade com contexto do usuário (assíncrono)
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        public override async Task<TEntity> CreateAsync(TCreateDto createDto)
        {
            var entity = await base.CreateAsync(createDto);
            SetUserContext(entity);
            return entity;
        }

        /// <summary>
        /// Define o contexto do usuário na entidade
        /// </summary>
        /// <param name="entity">Entidade a ser configurada</param>
        protected virtual void SetUserContext(TEntity entity)
        {
            var entityType = typeof(TEntity);
            
            var createdByProperty = entityType.GetProperty("CreatedById");
            createdByProperty?.SetValue(entity, _currentUserId);
            
            var uploadedByProperty = entityType.GetProperty("UploaderId");
            uploadedByProperty?.SetValue(entity, _currentUserId);
            
            var responsavelProperty = entityType.GetProperty("ResponsavelUserId");
            responsavelProperty?.SetValue(entity, _currentUserId);
        }
    }
}
