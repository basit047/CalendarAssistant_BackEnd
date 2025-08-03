namespace CalendarAssistant.Models
{
    public class ScheduleModel
    {
        public int Id { get; set; }
        public string? Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsWorkingDay { get; set; }
        public int UserId { get; set; }
    }

    public class TimeZone
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public class UserTimeZoneMapping
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? TimeZoneId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class UserTimeZoneMappingSaveModel
    {
        public int UserId { get; set; }
        public string? TimeZoneId { get; set; }
    }

    public class UserSyncModel
    {
        public int Id { get; set; }

        public DateTime SyncDateTime { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }
    }
}
