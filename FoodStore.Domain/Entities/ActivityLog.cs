using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities;

public partial class ActivityLog
{
    public string LogId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? Action { get; set; }

    public string? TagetTable { get; set; }

    public string? TargetId { get; set; }

    public string? TargetName { get; set; }

    public DateTime? TimeStamp { get; set; }

    public virtual User? User { get; set; }
}
