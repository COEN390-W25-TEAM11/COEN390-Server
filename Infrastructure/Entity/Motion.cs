using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Entity;

public class Motion {
    public required Guid Id { get; set; }

    public required DateTime DateTime { get; set; }
    public required bool motion { get; set; }

    public required Sensor Sensor { get; set; }
}
