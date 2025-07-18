using IntranetDocumentos.Application.DTOs.Common;

namespace IntranetDocumentos.Application.Factories
{
    /// <summary>
    /// Interface base para todas as factories
    /// Segue o padrão Factory Method para criação de objetos
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade a ser criada</typeparam>
    /// <typeparam name="TCreateDto">Tipo do DTO de criação</typeparam>
    public interface IFactory<TEntity, TCreateDto>
        where TEntity : class
        where TCreateDto : BaseDTO
    {
        /// <summary>
        /// Cria uma nova instância da entidade a partir do DTO
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade</returns>
        TEntity Create(TCreateDto createDto);

        /// <summary>
        /// Cria uma nova instância da entidade com validação
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <returns>Nova instância da entidade validada</returns>
        Task<TEntity> CreateAsync(TCreateDto createDto);

        /// <summary>
        /// Valida se o DTO é válido para criação
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <returns>True se válido, False caso contrário</returns>
        bool IsValid(TCreateDto createDto);

        /// <summary>
        /// Valida se o DTO é válido para criação com mensagens de erro
        /// </summary>
        /// <param name="createDto">DTO a ser validado</param>
        /// <param name="errors">Lista de erros encontrados</param>
        /// <returns>True se válido, False caso contrário</returns>
        bool IsValid(TCreateDto createDto, out List<string> errors);
    }

    /// <summary>
    /// Interface para factories que suportam diferentes tipos de criação
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade a ser criada</typeparam>
    /// <typeparam name="TCreateDto">Tipo do DTO de criação</typeparam>
    /// <typeparam name="TType">Tipo de criação (enum)</typeparam>
    public interface ITypedFactory<TEntity, TCreateDto, TType>
        where TEntity : class
        where TCreateDto : BaseDTO
        where TType : Enum
    {
        /// <summary>
        /// Cria uma nova instância da entidade baseada no tipo
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <param name="type">Tipo de criação</param>
        /// <returns>Nova instância da entidade</returns>
        TEntity Create(TCreateDto createDto, TType type);

        /// <summary>
        /// Cria uma nova instância da entidade baseada no tipo com validação
        /// </summary>
        /// <param name="createDto">DTO com dados para criação</param>
        /// <param name="type">Tipo de criação</param>
        /// <returns>Nova instância da entidade validada</returns>
        Task<TEntity> CreateAsync(TCreateDto createDto, TType type);

        /// <summary>
        /// Verifica se o tipo de criação é suportado
        /// </summary>
        /// <param name="type">Tipo a ser verificado</param>
        /// <returns>True se suportado, False caso contrário</returns>
        bool SupportsType(TType type);
    }

    /// <summary>
    /// Interface para factories que criam múltiplos objetos
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade a ser criada</typeparam>
    /// <typeparam name="TCreateDto">Tipo do DTO de criação</typeparam>
    public interface IBatchFactory<TEntity, TCreateDto>
        where TEntity : class
        where TCreateDto : BaseDTO
    {
        /// <summary>
        /// Cria múltiplas instâncias da entidade
        /// </summary>
        /// <param name="createDtos">Lista de DTOs para criação</param>
        /// <returns>Lista de entidades criadas</returns>
        List<TEntity> CreateMany(IEnumerable<TCreateDto> createDtos);

        /// <summary>
        /// Cria múltiplas instâncias da entidade com validação
        /// </summary>
        /// <param name="createDtos">Lista de DTOs para criação</param>
        /// <returns>Lista de entidades criadas validadas</returns>
        Task<List<TEntity>> CreateManyAsync(IEnumerable<TCreateDto> createDtos);

        /// <summary>
        /// Valida uma lista de DTOs para criação em lote
        /// </summary>
        /// <param name="createDtos">Lista de DTOs para validação</param>
        /// <param name="errors">Dicionário com erros por índice</param>
        /// <returns>True se todos válidos, False caso contrário</returns>
        bool ValidateBatch(IEnumerable<TCreateDto> createDtos, out Dictionary<int, List<string>> errors);
    }
}
