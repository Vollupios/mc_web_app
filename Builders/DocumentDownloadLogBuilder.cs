using IntranetDocumentos.Models;

namespace IntranetDocumentos.Builders
{
    public static class DocumentDownloadLogBuilder
    {
        public static DocumentDownloadLog Build(int documentId, string userId, string userAgent, string ipAddress)
        {
            return DocumentDownloadLog.CreateSuccessful(documentId, userId, userAgent, ipAddress, 0);
        }
        
        public static DocumentDownloadLog BuildWithFileSize(int documentId, string userId, string userAgent, string ipAddress, long fileSize)
        {
            return DocumentDownloadLog.CreateSuccessful(documentId, userId, userAgent, ipAddress, fileSize);
        }
        
        public static DocumentDownloadLog BuildFailed(int documentId, string userId, string errorMessage, string userAgent, string ipAddress)
        {
            return DocumentDownloadLog.CreateFailed(documentId, userId, errorMessage, userAgent, ipAddress);
        }
    }
}