using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class LoginViewModelBuilder
    {
        public static LoginViewModel Build(string? email = null)
        {
            return new LoginViewModel
            {
                Email = email ?? string.Empty
            };
        }
    }
}
