using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class CreateUserViewModelBuilder
    {
        public static CreateUserViewModel Build(List<Department> departments, List<string> roles)
        {
            return new CreateUserViewModel
            {
                AvailableDepartments = departments,
                AvailableRoles = roles
            };
        }
    }
}
