using IntranetDocumentos.Models;

namespace IntranetDocumentos.Builders
{
    public static class DocumentDownloadLogBuilder
    {
        public static DocumentDownloadLog Build(int documentId, string userId, string userAgent, string ipAddress)
        {
            return new DocumentDownloadLog
            {
                DocumentId = documentId,
                UserId = userId,
                DownloadDate = DateTime.Now,
                UserAgent = userAgent,
                IpAddress = ipAddress
            };
        }
    }
}