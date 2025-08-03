using CalendarAssistant.Models;

namespace CalendarAssistant.Services
{
    public interface IScheduleService
    {
        Task<List<ScheduleModel>> GetWeeklySchedule(int userId);
        Task<bool> SaveWeeklySchedule(List<ScheduleModel> scheduleModel);
        Task<List<Models.TimeZone>> GetAllTimeZone();
        Task<UserTimeZoneMapping> GetUserTimeZoneMapping(int userId);
        Task<bool> SaveUserTimeZoneMapping(UserTimeZoneMappingSaveModel userTimeZoneMappingSaveModel);
    }
}
