using CalendarAssistant.Models;
using Google.Apis.Calendar.v3.Data;

namespace CalendarAssistant.Services
{
    public interface IGoogleCalendarService
    {
        Task<List<EventListView>> GetAllEvents(string accessToken);
        Task<bool> Schedule(CalendarEvent calendarEvent, CancellationToken cancellationToken);
        Task<bool> Cancel(string eventId, CancellationToken cancellationToken);
        Task<bool> Reschedule(UpdateEvent updateEvent, string accessToken);
        Task<Event> GetEventById(string eventId, string accessToken);
        Task<List<Event>> GetEventByDay(DateTime startDate, DateTime endDate, string accessToken, string eventIdToExclude = "");
        Task<List<Event>> GetConflictingEvents(DateTime startDate, DateTime endDate, string accessToken, string eventIdToExclude = "");
        Task<DateTime?> FindAvailableSlot(DateTime searchStart, DateTime searchEnd, TimeSpan requiredDuration, string accessToken, string timeZone = "UTC");
        Task<UserAuthentication> AuthenticateUser(Models.TokenRequest request);
    }
}
