using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class DocumentBuilder
    {
        public static Document BuildFromUpload(string originalFileName, string storedFileName, string contentType, long fileSize, string uploaderId, int? departmentId, string? contentText)
        {
            return new Document
            {
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = contentType,
                FileSize = fileSize,
                UploadDate = DateTime.Now,
                UploaderId = uploaderId,
                DepartmentId = departmentId,
                ContentText = contentText
            };
        }
    }
}
