using CalendarAssistant.Services;

namespace CalendarAssistant.Models
{
    public class UserService : IUserService
    {
        private string? _userName;
        private int _userId;
        private string? _userEmail;

        public void SetUserDetails(string? userName, int userId, string? userEmail)
        {
            _userName = userName;
            _userId = userId;
            _userEmail = userEmail;
        }

        public UserDetail GetUserDetails()
        {
            return new UserDetail()
            {
                UserId = _userId,
                UserName = _userName,
                UserEmail = _userEmail,
            };

        }
    }
}
