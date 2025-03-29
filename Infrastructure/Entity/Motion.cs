using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Entity;

public class Motion {
    public Guid Id { get; set; }

    public DateTime DateTime { get; set; }
    public bool motion { get; set; }

    public Sensor Sensor { get; set; }
}
