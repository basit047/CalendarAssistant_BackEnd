using CalendarAssistant.Models;

namespace CalendarAssistant.Services
{
    public interface IUserService
    {
        void SetUserDetails(string? userName, int userId, string? userEmail);
        UserDetail GetUserDetails();
    }
}
