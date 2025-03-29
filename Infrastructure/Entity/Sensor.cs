namespace Infrastructure.Entity;

public class Sensor {
    public Guid Id { get; set; }
    public Guid EspId { get; set; }
    public int Pin { get; set; }
    public string Name { get; set; }

    public int Sensitivity { get; set; }
    public int Timeout { get; set; }

    public List<Motion> MotionHistory { get; set; }
    public List<Assigned> assigned { get; set;}
}
