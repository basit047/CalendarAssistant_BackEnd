using CalendarAssistant.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CalendarAssistant.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly CalendarAssistantContext _calendarAssistantContext;

        public ScheduleService(CalendarAssistantContext calendarAssistantContext)
        {
            _calendarAssistantContext = calendarAssistantContext;
        }


        public async Task<List<ScheduleModel>> GetWeeklySchedule(int userId)
        {
            return await _calendarAssistantContext.Set<ScheduleModel>()
                         .FromSqlRaw($@"
                            SELECT dw.Id, dw.Day, wh.StartTime, wh.EndTime, wh.IsWorkingDay, asp.UserId
                            FROM DayWeek dw
                            INNER JOIN WorkingHour wh ON dw.Id = wh.DayId
                            INNER JOIN AspNetUsers asp ON wh.UserId = asp.UserId
                            WHERE asp.UserId={userId}
                        ").ToListAsync();
        }

        public async Task<bool> SaveWeeklySchedule(List<ScheduleModel> scheduleModel)
        {
            if (scheduleModel == null)
                return false;

            int result = 0;
            var existingSchedule = _calendarAssistantContext.WorkingHours.Where(x => x.UserId == scheduleModel.First().UserId);
            _calendarAssistantContext.WorkingHours.RemoveRange(existingSchedule);
            result = await _calendarAssistantContext.SaveChangesAsync();

            List<WorkingHour> list = new List<WorkingHour>();

            foreach (var item in scheduleModel)
            {
                var workingHourModel = new WorkingHour()
                {
                    DayId = item.Id,
                    EndTime = item.EndTime,
                    IsWorkingDay = item.IsWorkingDay,
                    StartTime = item.StartTime,
                    UserId = item.UserId
                };

                list.Add(workingHourModel);
            }

            await _calendarAssistantContext.WorkingHours.AddRangeAsync(list);
            result = await _calendarAssistantContext.SaveChangesAsync();

            return result > 0 ? true : false;
        }

        public async Task<List<Models.TimeZone>> GetAllTimeZone()
        {
            return await Task.Run(() =>
            {
                return TimeZoneInfo.GetSystemTimeZones().Select(p => new Models.TimeZone
                {
                    Id = p.Id,
                    Name = p.DisplayName
                }).ToList();
            });
        }

        public async Task<UserTimeZoneMapping> GetUserTimeZoneMapping(int userId)
        {
            var result = await _calendarAssistantContext.AspNetUsers.Where(x => x.UserId == userId).Select(p => new Models.UserTimeZoneMapping
            {
                UserId = p.UserId,
                TimeZoneId = p.TimeZoneId,
                UserName = p.UserName,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt ?? p.CreatedAt
            }).FirstOrDefaultAsync();

            return result!;
        }

        public async Task<bool> SaveUserTimeZoneMapping(UserTimeZoneMappingSaveModel userTimeZoneMappingSaveModel)
        {
            var user = await _calendarAssistantContext.AspNetUsers.FirstOrDefaultAsync(x => x.UserId == userTimeZoneMappingSaveModel.UserId);
            if (user == null)
                return false;

            user.TimeZoneId = userTimeZoneMappingSaveModel.TimeZoneId;
            user.ModifiedAt = DateTime.UtcNow;

            var result = await _calendarAssistantContext.SaveChangesAsync();

            return result > 0 ? true : false;
        }
    }
}
