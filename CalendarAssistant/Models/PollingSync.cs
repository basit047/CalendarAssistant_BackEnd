using System;
using System.Collections.Generic;

namespace CalendarAssistant.Models;

public partial class PollingSync
{
    public int Id { get; set; }

    public DateTime SyncDateTime { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
