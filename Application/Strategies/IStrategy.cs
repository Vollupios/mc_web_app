using System;

namespace IntranetDocumentos.Application.Strategies
{
    /// <summary>
    /// Interface base para implementação do Strategy Pattern
    /// </summary>
    /// <typeparam name="TInput">Tipo de entrada para o processamento</typeparam>
    /// <typeparam name="TOutput">Tipo de saída após o processamento</typeparam>
    public interface IStrategy<in TInput, out TOutput>
    {
        /// <summary>
        /// Executa a estratégia de processamento
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <returns>Resultado do processamento</returns>
        TOutput Execute(TInput input);
    }

    /// <summary>
    /// Interface para estratégias assíncronas
    /// </summary>
    /// <typeparam name="TInput">Tipo de entrada</typeparam>
    /// <typeparam name="TOutput">Tipo de saída</typeparam>
    public interface IAsyncStrategy<in TInput, TOutput>
    {
        /// <summary>
        /// Executa a estratégia de processamento de forma assíncrona
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resultado do processamento</returns>
        Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface para estratégias com contexto
    /// </summary>
    /// <typeparam name="TInput">Tipo de entrada</typeparam>
    /// <typeparam name="TOutput">Tipo de saída</typeparam>
    /// <typeparam name="TContext">Tipo do contexto</typeparam>
    public interface IContextStrategy<in TInput, out TOutput, in TContext>
    {
        /// <summary>
        /// Executa a estratégia com contexto
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <param name="context">Contexto da operação</param>
        /// <returns>Resultado do processamento</returns>
        TOutput Execute(TInput input, TContext context);
    }

    /// <summary>
    /// Interface para estratégias com validação
    /// </summary>
    /// <typeparam name="TInput">Tipo de entrada</typeparam>
    /// <typeparam name="TOutput">Tipo de saída</typeparam>
    public interface IValidatedStrategy<in TInput, out TOutput> : IStrategy<TInput, TOutput>
    {
        /// <summary>
        /// Valida a entrada antes do processamento
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <returns>True se válido, False caso contrário</returns>
        bool IsValid(TInput input);

        /// <summary>
        /// Obtém os erros de validação
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <returns>Lista de erros de validação</returns>
        IEnumerable<string> GetValidationErrors(TInput input);
    }

    /// <summary>
    /// Interface para estratégias com múltiplas etapas
    /// </summary>
    /// <typeparam name="TInput">Tipo de entrada</typeparam>
    /// <typeparam name="TIntermediate">Tipo intermediário</typeparam>
    /// <typeparam name="TOutput">Tipo de saída</typeparam>
    public interface IPipelineStrategy<in TInput, TIntermediate, out TOutput>
    {
        /// <summary>
        /// Primeira etapa do pipeline
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <returns>Resultado intermediário</returns>
        TIntermediate ProcessFirst(TInput input);

        /// <summary>
        /// Etapa final do pipeline
        /// </summary>
        /// <param name="intermediate">Resultado intermediário</param>
        /// <returns>Resultado final</returns>
        TOutput ProcessFinal(TIntermediate intermediate);

        /// <summary>
        /// Executa todo o pipeline
        /// </summary>
        /// <param name="input">Dados de entrada</param>
        /// <returns>Resultado final</returns>
        TOutput Execute(TInput input);
    }
}
