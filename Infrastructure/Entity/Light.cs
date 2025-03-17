namespace Infrastructure.Entity;

public class Light {
    public Guid Id { get; set; }
    public string Name { get; set; }

    public bool Overide { get; set; }
    public int State { get; set; }

    public List<Motion> MotionHistory { get; set; }
}
