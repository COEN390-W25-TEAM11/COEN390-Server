namespace Infrastructure.Entity;

public class Sensor {
    public required Guid Id { get; set; }
    public required Guid EspId { get; set; }
    public required int Pin { get; set; }
    public required string Name { get; set; }

    public required int Sensitivity { get; set; }
    public required int Timeout { get; set; }

    public required List<Motion> MotionHistory { get; set; }
    public required List<Assigned> assigned { get; set;}
}
