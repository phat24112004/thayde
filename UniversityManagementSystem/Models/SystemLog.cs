using System;
using System.Collections.Generic;

namespace UniversityManagementSystem.Models;

public partial class SystemLog
{
    public int LogId { get; set; }

    public string? Action { get; set; }

    public string? PerformedBy { get; set; }

    public DateTime? PerformedTime { get; set; }

    public string? Details { get; set; }
}
