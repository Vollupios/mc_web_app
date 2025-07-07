using IntranetDocumentos.Services.FileProcessing;

namespace IntranetDocumentos.Services.FileProcessing
{
    /// <summary>
    /// Factory para criação de processadores de arquivo - Factory Pattern
    /// Implementa Single Responsibility Principle (SRP)
    /// </summary>
    public class FileProcessorFactory
    {
        private readonly IEnumerable<IFileProcessor> _processors;
        private readonly ILogger<FileProcessorFactory> _logger;

        public FileProcessorFactory(IEnumerable<IFileProcessor> processors, ILogger<FileProcessorFactory> logger)
        {
            _processors = processors;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o processador apropriado para a extensão do arquivo
        /// Prioriza processadores específicos sobre o processador genérico
        /// </summary>
        public IFileProcessor GetProcessor(string fileExtension)
        {
            // Primeiro, tentar encontrar um processador específico (não genérico)
            var specificProcessor = _processors
                .Where(p => p.GetType().Name != "GenericFileProcessor")
                .FirstOrDefault(p => p.CanProcess(fileExtension));
            
            if (specificProcessor != null)
            {
                _logger.LogDebug("Processador específico encontrado para {Extension}: {ProcessorType}", 
                    fileExtension, specificProcessor.GetType().Name);
                return specificProcessor;
            }

            // Se não encontrou processador específico, usar o genérico
            var genericProcessor = _processors
                .FirstOrDefault(p => p.GetType().Name == "GenericFileProcessor");
            
            if (genericProcessor != null)
            {
                _logger.LogDebug("Usando processador genérico para {Extension}", fileExtension);
                return genericProcessor;
            }

            // Se nem o genérico existe, lançar exceção
            _logger.LogWarning("Nenhum processador encontrado para extensão: {Extension}", fileExtension);
            throw new NotSupportedException($"Tipo de arquivo {fileExtension} não é suportado");
        }

        /// <summary>
        /// Obtém todos os processadores disponíveis
        /// </summary>
        public IEnumerable<IFileProcessor> GetAllProcessors()
        {
            return _processors;
        }

        /// <summary>
        /// Obtém todas as extensões suportadas
        /// </summary>
        public IEnumerable<string> GetSupportedExtensions()
        {
            return _processors.SelectMany(p => p.SupportedExtensions).Distinct();
        }

        /// <summary>
        /// Verifica se uma extensão é suportada
        /// </summary>
        public bool IsExtensionSupported(string fileExtension)
        {
            return _processors.Any(p => p.CanProcess(fileExtension));
        }

        /// <summary>
        /// Obtém informações sobre processadores e extensões suportadas
        /// </summary>
        public Dictionary<string, object> GetProcessorInfo()
        {
            var info = new Dictionary<string, object>();
            
            foreach (var processor in _processors)
            {
                var processorName = processor.GetType().Name;
                info[processorName] = new
                {
                    SupportedExtensions = processor.SupportedExtensions,
                    MaxFileSize = processor.MaxFileSize,
                    MaxFileSizeMB = processor.MaxFileSize / (1024.0 * 1024.0)
                };
            }
            
            return info;
        }
    }
}
