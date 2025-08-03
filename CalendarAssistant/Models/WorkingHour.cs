namespace CalendarAssistant.Models;

public partial class WorkingHour
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int DayId { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public bool? IsWorkingDay { get; set; }

    public virtual DayWeek Day { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
