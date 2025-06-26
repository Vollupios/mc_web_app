using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class UploadViewModelBuilder
    {
        public static UploadViewModel Build(List<Department> departments)
        {
            return new UploadViewModel
            {
                AvailableDepartments = departments
            };
        }
    }
}
