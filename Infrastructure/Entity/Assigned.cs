namespace Infrastructure.Entity;

public class Assigned {
    public required Guid Id { get; set; }
    public required Light Light { get; set; }
    public required Sensor Sensor { get; set; }
}
