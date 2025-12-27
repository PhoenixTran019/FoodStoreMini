using System;
using System.Collections.Generic;

namespace FoodStore.Domain.Entities
    ;

public partial class UserProfile
{
    public string ProfileId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? StaffPhone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual User? User { get; set; }
}
