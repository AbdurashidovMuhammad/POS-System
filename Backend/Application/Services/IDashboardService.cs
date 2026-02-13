using Application.DTOs.DashboardDTOs;

namespace Application.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
