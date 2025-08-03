using System;
using System.Collections.Generic;

namespace CalendarAssistant.Models;

public partial class DayWeek
{
    public int Id { get; set; }

    public string Day { get; set; } = null!;

    public virtual ICollection<WorkingHour> WorkingHours { get; set; } = new List<WorkingHour>();
}
