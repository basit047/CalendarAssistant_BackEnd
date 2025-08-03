using System;
using System.Collections.Generic;

namespace CalendarAssistant.Models;

public partial class Meeting
{
    public int MeetingId { get; set; }

    public string EventId { get; set; } = null!;

    public string? Attendees { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public int StatusId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual AspNetUser CreatedByNavigation { get; set; } = null!;

    public virtual AspNetUser? ModifiedByNavigation { get; set; }
}
