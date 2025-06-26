using IntranetDocumentos.Models;
using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class RamalViewModelBuilder
    {
        public static RamalViewModel Build(Ramal ramal, List<Department> departments)
        {
            return new RamalViewModel
            {
                Id = ramal.Id,
                Numero = ramal.Numero,
                Nome = ramal.Nome,
                TipoFuncionario = ramal.TipoFuncionario,
                DepartmentId = ramal.DepartmentId,
                Observacoes = ramal.Observacoes,
                Ativo = ramal.Ativo,
                FotoPath = ramal.FotoPath,
                AvailableDepartments = departments
            };
        }
    }
}
