using IntranetDocumentos.Models.ViewModels;

namespace IntranetDocumentos.Builders
{
    public static class DashboardViewModelBuilder
    {
        public static DashboardViewModel Build(
            DocumentStatisticsViewModel docStats,
            ReunioesMetricsViewModel reunioesMetrics,
            DepartmentActivityViewModel departmentActivity)
        {
            return new DashboardViewModel
            {
                DocumentStatistics = docStats,
                ReunioesMetrics = reunioesMetrics,
                DepartmentActivity = departmentActivity
            };
        }
    }
}
