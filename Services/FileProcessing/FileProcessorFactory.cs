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
        /// </summary>
        public IFileProcessor GetProcessor(string fileExtension)
        {
            var processor = _processors.FirstOrDefault(p => p.CanProcess(fileExtension));
            
            if (processor == null)
            {
                _logger.LogWarning("Nenhum processador encontrado para extensão: {Extension}", fileExtension);
                throw new NotSupportedException($"Tipo de arquivo {fileExtension} não é suportado");
            }

            _logger.LogDebug("Processador encontrado para {Extension}: {ProcessorType}", 
                fileExtension, processor.GetType().Name);
            
            return processor;
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
